using PromeRotation.Data;
using PromeRotation.Rotation;

namespace PromeRotation.Sample.Events;

public sealed class SampleRotationEventHandler : IRotationEventHandler
{
        public void OnUpdate()
    {
    }

        public void OnOutOfBattleUpdate()
    {
    }

        public void OnBattleStarted()
    {
    }

        public void OnBattleUpdate()
    {
    }

        public void OnNoTarget()
    {
    }

        public void OnBattleEnded()
    {
        PromeSettings.Instance.OpenerHasBeenExecuted = false;
    }

        public void OnTerritoryChanged(ushort territoryId)
    {
    }
}
