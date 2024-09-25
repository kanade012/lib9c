using System;
using Bencodex.Types;
using Nekoyume.Model.Market;
using Nekoyume.Model.State;
using static Lib9c.SerializeKeys;


namespace Nekoyume.Model.Mail
{
    [Serializable]
    public class ProductBuyerMail : Mail
    {
        public const string ProductKey = "p";
        public readonly Guid ProductId;
        public readonly Product Product;
        public ProductBuyerMail(long blockIndex, Guid id, long requiredBlockIndex, Guid productId, Product product) : base(blockIndex, id, requiredBlockIndex)
        {
            ProductId = productId;
            Product = product;
        }

        public ProductBuyerMail(Dictionary serialized) : base(serialized)
        {
            ProductId = serialized[ProductIdKey].ToGuid();
            if (serialized.ContainsKey(ProductKey))
            {
                Product = ProductFactory.DeserializeProduct((List) serialized[ProductKey]);
            }
        }

        public override void Read(IMail mail)
        {
            mail.Read(this);
        }

        public override MailType MailType => MailType.Auction;

        protected override string TypeId => nameof(ProductBuyerMail);

        public override IValue Serialize() => ((Dictionary)base.Serialize())
            .Add(ProductIdKey, ProductId.Serialize())
            .Add(ProductKey, Product.Serialize());
    }
}
