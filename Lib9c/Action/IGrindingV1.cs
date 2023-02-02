#nullable enable

using System;
using System.Collections.Generic;
using Libplanet;

namespace Nekoyume.Action
{
    public interface IGrindingV1
    {
        Address AvatarAddress { get; }
        List<Guid> EquipmentsIds { get; }
        bool ChargeAp { get; }
    }
}
