using UnityEngine;

namespace Project.Scripts.Grid
{
    public class HeatMapDataObject
    {
        private const int MinValue = 0, MaxValue = 100;

        private readonly GridField<HeatMapDataObject> _gridField;
        private readonly Vector2Int _position;
        public float Value { get; private set; } = 0;

        public HeatMapDataObject(GridField<HeatMapDataObject> gridField, Vector2Int position)
        {
            _gridField = gridField;
            _position = position;
        }
        public HeatMapDataObject(GridField<HeatMapDataObject> gridField, int x, int y)
        {
            _gridField = gridField;
            _position = new Vector2Int(x,y);
        }

        public void AddValue(float val)
        {
            Value += val;
            Value = Mathf.Clamp(Value, MinValue, MaxValue);
            _gridField.TriggerGridObjectChanged(_position);
        }

        public float GetNormalizedValue()
        {
            return Value / MaxValue;
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}
