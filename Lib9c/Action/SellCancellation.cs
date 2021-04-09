using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Bencodex.Types;
using Libplanet;
using Libplanet.Action;
using Nekoyume.Model.Item;
using Nekoyume.Model.Mail;
using Nekoyume.Model.State;
using Serilog;
using static Lib9c.SerializeKeys;

namespace Nekoyume.Action
{
    [Serializable]
    [ActionType("sell_cancellation5")]
    public class SellCancellation : GameAction
    {
        public Guid productId;
        public Address sellerAvatarAddress;
        public Result result;
        public ItemSubType itemSubType;

        [Serializable]
        public class Result : AttachmentActionResult
        {
            public ShopItem shopItem;
            public Guid id;

            protected override string TypeId => "sellCancellation.result";

            public Result()
            {
            }

            public Result(Bencodex.Types.Dictionary serialized) : base(serialized)
            {
                shopItem = new ShopItem((Bencodex.Types.Dictionary) serialized["shopItem"]);
                id = serialized["id"].ToGuid();
            }

            public override IValue Serialize() =>
#pragma warning disable LAA1002
                new Bencodex.Types.Dictionary(new Dictionary<IKey, IValue>
                {
                    [(Text) "shopItem"] = shopItem.Serialize(),
                    [(Text) "id"] = id.Serialize()
                }.Union((Bencodex.Types.Dictionary) base.Serialize()));
#pragma warning restore LAA1002
        }

        protected override IImmutableDictionary<string, IValue> PlainValueInternal => new Dictionary<string, IValue>
        {
            ["productId"] = productId.Serialize(),
            ["sellerAvatarAddress"] = sellerAvatarAddress.Serialize(),
            ["itemSubType"] = itemSubType.Serialize(),
        }.ToImmutableDictionary();

        protected override void LoadPlainValueInternal(IImmutableDictionary<string, IValue> plainValue)
        {
            productId = plainValue["productId"].ToGuid();
            sellerAvatarAddress = plainValue["sellerAvatarAddress"].ToAddress();
            itemSubType = plainValue["itemSubType"].ToEnum<ItemSubType>();
        }

        public override IAccountStateDelta Execute(IActionContext context)
        {
            IActionContext ctx = context;
            var states = ctx.PreviousStates;
            Address shardedShopAddress = ShardedShopState.DeriveAddress(itemSubType, productId);
            if (ctx.Rehearsal)
            {
                states = states.SetState(shardedShopAddress, MarkChanged);
                return states
                    .SetState(Addresses.Shop, MarkChanged)
                    .SetState(sellerAvatarAddress, MarkChanged);
            }

            var addressesHex = GetSignerAndOtherAddressesHex(context, sellerAvatarAddress);

            var sw = new Stopwatch();
            sw.Start();
            var started = DateTimeOffset.UtcNow;
            Log.Verbose("{AddressesHex}Sell Cancel exec started", addressesHex);

            if (!states.TryGetAvatarState(ctx.Signer, sellerAvatarAddress, out var avatarState))
            {
                throw new FailedLoadStateException(
                    $"{addressesHex}Aborted as the avatar state of the seller was failed to load.");
            }
            sw.Stop();
            Log.Verbose("{AddressesHex}Sell Cancel Get AgentAvatarStates: {Elapsed}", addressesHex, sw.Elapsed);
            sw.Restart();

            if (!avatarState.worldInformation.IsStageCleared(GameConfig.RequireClearedStageLevel.ActionsInShop))
            {
                avatarState.worldInformation.TryGetLastClearedStageId(out var current);
                throw new NotEnoughClearedStageLevelException(addressesHex,
                    GameConfig.RequireClearedStageLevel.ActionsInShop, current);
            }

            if (!states.TryGetState(shardedShopAddress, out Dictionary shopStateDict))
            {
                ShardedShopState shopState = new ShardedShopState(shardedShopAddress);
                shopStateDict = (Dictionary) shopState.Serialize();
            }
            sw.Stop();
            Log.Verbose("{AddressesHex}Sell Cancel Get ShopState: {Elapsed}", addressesHex, sw.Elapsed);
            sw.Restart();

            // 상점에서 아이템을 빼온다.
            List products = (List)shopStateDict[ProductsKey];

            IValue productIdSerialized = productId.Serialize();
            Dictionary productSerialized = products
                .Select(p => (Dictionary) p)
                .FirstOrDefault(p => p[ProductIdKey].Equals(productIdSerialized));

            bool backwardCompatible = false;
            if (productSerialized.Equals(Dictionary.Empty))
            {
                // Backward compatibility.
                IValue rawShop = states.GetState(Addresses.Shop);
                if (!(rawShop is null))
                {
                    Dictionary legacyShopDict = (Dictionary) rawShop;
                    Dictionary legacyProducts = (Dictionary) legacyShopDict[LegacyProductsKey];
                    IKey productKey = (IKey) productId.Serialize();
                    // SoldOut
                    if (!legacyProducts.ContainsKey(productKey))
                    {
                        throw new ItemDoesNotExistException(
                            $"{addressesHex}Aborted as the shop item ({productId}) was failed to get from the legacy shop."
                        );
                    }

                    productSerialized = (Dictionary) legacyProducts[productKey];
                    legacyProducts = (Dictionary) legacyProducts.Remove(productKey);
                    legacyShopDict = legacyShopDict.SetItem(LegacyProductsKey, legacyProducts);
                    states = states.SetState(Addresses.Shop, legacyShopDict);
                    backwardCompatible = true;
                }
            }
            else
            {
                products = (List) products.Remove(productSerialized);
                shopStateDict = shopStateDict.SetItem(ProductsKey, new List<IValue>(products));
            }
            ShopItem shopItem = new ShopItem(productSerialized);

            sw.Stop();
            Log.Verbose("{AddressesHex}Sell Cancel Get Unregister Item: {Elapsed}", addressesHex, sw.Elapsed);
            sw.Restart();

            if (shopItem.SellerAvatarAddress != sellerAvatarAddress || shopItem.SellerAgentAddress != ctx.Signer)
            {
                throw new InvalidAddressException($"{addressesHex}Invalid Avatar Address");
            }

            INonFungibleItem nonFungibleItem = (INonFungibleItem)shopItem.ItemUsable ?? shopItem.Costume;
            if (avatarState.inventory.TryGetNonFungibleItem(nonFungibleItem.ItemId, out INonFungibleItem outNonFungibleItem))
            {
                outNonFungibleItem.Update(ctx.BlockIndex);
            }
            nonFungibleItem.Update(ctx.BlockIndex);

            if (backwardCompatible)
            {
                switch (nonFungibleItem)
                {
                    case ItemUsable itemUsable:
                        avatarState.UpdateFromAddItem(itemUsable, true);
                        break;
                    case Costume costume:
                        avatarState.UpdateFromAddCostume(costume, true);
                        break;
                }
            }
            // 메일에 아이템을 넣는다.
            result = new Result
            {
                shopItem = shopItem,
                itemUsable = shopItem.ItemUsable,
                costume = shopItem.Costume
            };
            var mail = new SellCancelMail(result, ctx.BlockIndex, ctx.Random.GenerateRandomGuid(), ctx.BlockIndex);
            result.id = mail.id;

            avatarState.UpdateV4(mail, context.BlockIndex);
            avatarState.updatedAt = ctx.BlockIndex;
            avatarState.blockIndex = ctx.BlockIndex;

            sw.Stop();
            Log.Verbose("{AddressesHex}Sell Cancel Update AvatarState: {Elapsed}", addressesHex, sw.Elapsed);
            sw.Restart();

            states = states.SetState(sellerAvatarAddress, avatarState.Serialize());
            sw.Stop();
            Log.Verbose("{AddressesHex}Sell Cancel Set AvatarState: {Elapsed}", addressesHex, sw.Elapsed);
            sw.Restart();

            states = states.SetState(shardedShopAddress, shopStateDict);
            sw.Stop();
            var ended = DateTimeOffset.UtcNow;
            Log.Verbose("{AddressesHex}Sell Cancel Set ShopState: {Elapsed}", addressesHex, sw.Elapsed);
            Log.Verbose("{AddressesHex}Sell Cancel Total Executed Time: {Elapsed}", addressesHex, ended - started);
            return states;
        }
    }
}
