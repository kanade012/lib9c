namespace Lib9c.Tests.Action
{
    using System.Collections.Generic;
    using System.Linq;
    using Libplanet.Action.State;
    using Libplanet.Crypto;
    using Nekoyume;
    using Nekoyume.Action;
    using Nekoyume.Model.Item;
    using Nekoyume.Model.State;
    using Nekoyume.Module;
    using Nekoyume.TableData;
    using Xunit;

    public class ChargeActionPoint0Test
    {
        private readonly Dictionary<string, string> _sheets;
        private readonly TableSheets _tableSheets;

        public ChargeActionPoint0Test()
        {
            _sheets = TableSheetsImporter.ImportSheets();
            _tableSheets = new TableSheets(_sheets);
        }

        [Fact]
        public void Execute()
        {
            var privateKey = new PrivateKey();
            var agentAddress = privateKey.PublicKey.Address;
            var agent = new AgentState(agentAddress);

            var avatarAddress = agentAddress.Derive("avatar");
            var gameConfigState = new GameConfigState(_sheets[nameof(GameConfigSheet)]);
            var avatarState = new AvatarState(
                avatarAddress,
                agentAddress,
                0,
                _tableSheets.GetAvatarSheets(),
                gameConfigState,
                default
            )
            {
                actionPoint = 0,
            };
            agent.avatarAddresses.Add(0, avatarAddress);

            var apStone =
                ItemFactory.CreateItem(
                    _tableSheets.MaterialItemSheet.Values.First(r => r.ItemSubType == ItemSubType.ApStone),
                    new TestRandom());
            avatarState.inventory.AddItem2(apStone);

            Assert.Equal(0, avatarState.actionPoint);

            var state = new World(new MockWorldState())
                .SetLegacyState(Addresses.GameConfig, gameConfigState.Serialize())
                .SetAgentState(agentAddress, agent)
                .SetAvatarState(avatarAddress, avatarState);

            foreach (var (key, value) in _sheets)
            {
                state = state.SetLegacyState(Addresses.TableSheet.Derive(key), value.Serialize());
            }

            var action = new ChargeActionPoint0()
            {
                avatarAddress = avatarAddress,
            };

            var nextState = action.Execute(new ActionContext()
            {
                PreviousState = state,
                Signer = agentAddress,
                RandomSeed = 0,
            });

            var nextAvatarState = nextState.GetAvatarState(avatarAddress);

            Assert.Equal(gameConfigState.ActionPointMax, nextAvatarState.actionPoint);
        }
    }
}
