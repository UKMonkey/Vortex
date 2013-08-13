using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using SlimMath;

namespace Vortex.Interface.Traits
{
    public delegate void TraitCallback(Trait prop);
    /** Different types of property supported:
     *  float
     *  int
     *  short
     *  string
     *  
     *  note that adding others will involve changing this class directly
     *  or inheriting it in the game, since the cached values will offer quite a
     *  significant performance increase
     */
    public class Trait
    {
        public event TraitCallback OnTraitChanged;

        protected DataType DataType { get; set; }

            // the type of property this is
        public short PropertyId { get; set; }

            // has the value changed since the flag was last cleared?
        private bool _isDirty = true;

        public bool IsDirty
        {
            get { return IsDirtyable && _isDirty; }
            set { _isDirty = value; }
        }

        public bool IsDirtyable { get; set; }

        public bool IsPersistant { get; set; }

            // the data
        private byte[] _value;

        public byte[] Value
        {
            get { return _value; }
            set
            {
                if (!IsDirty && _value != null && _value.Length == value.Length)
                {
                    var dirty = false;
                    for (var i = 0; i < _value.Length && !dirty; ++i)
                        if (_value[i] != value[i])
                            dirty = true;
                    if (!dirty)
                        return;
                }

                _value = value;
                IsDirty = true;
                Reset();

                if (OnTraitChanged != null)
                    OnTraitChanged(this);
            }
        }

        private void SetDataType(DataType expectedType)
        {
            if (!(DataType == DataType.Unassigned || DataType == expectedType))
                Debug.Assert(false);
            DataType = expectedType;
        }

#region ByteArray
        // how to access the data
        public byte[] ByteArrayValue
        {
            get { return _value; }
            set
            {
                SetDataType(DataType.ByteArray);
                Value = value;
            }
        }
#endregion

#region String
        private string _stringValue;
        public string StringValue
        {
            get
            {
                if (_stringValue == null)
                    _stringValue = System.Text.Encoding.UTF8.GetString(Value);
                return _stringValue;
            }
            set
            {
                SetDataType(DataType.String);
                Value = System.Text.Encoding.UTF8.GetBytes(value);
            }
        }
#endregion

#region Int
        private int? _intValue;
        public int IntValue
        {
            get
            {
                if (_intValue == null)
                    _intValue = BitConverter.ToInt32(Value, 0);
                return _intValue.Value;
            }
            set
            {
                SetDataType(DataType.Int);
                Value = BitConverter.GetBytes(value);
            }
        }
#endregion

#region Float
        private float? _floatValue;
        public float FloatValue
        {
            get
            {
                if (_floatValue == null)
                    _floatValue = BitConverter.ToSingle(ByteArrayValue, 0);
                return _floatValue.Value;
            }
            set
            { 
                Value = BitConverter.GetBytes(value);
                DataType = DataType.Float;
            }
        }
#endregion

#region Short
        private short? _shortValue;
        public short ShortValue
        {
            get
            {
                if (_shortValue == null)
                    _shortValue = BitConverter.ToInt16(Value, 0);
                return _shortValue.Value;
            }
            set
            {
                SetDataType(DataType.Short);
                Value = BitConverter.GetBytes(value);
            }
        }
#endregion

#region Bool
        private bool? _boolValue;
        public bool BoolValue
        {
            get
            {
                if (_boolValue == null)
                    _boolValue = BitConverter.ToBoolean(ByteArrayValue, 0);
                return _boolValue.Value;
            }
            set
            {
                SetDataType(DataType.Bool);
                Value = BitConverter.GetBytes(value);
            }
        }
#endregion

#region Byte
        public byte ByteValue
        {
            get { return Value[0]; }
            set
            {
                SetDataType(DataType.Byte);
                Value = new[] { value };
            }
        }
#endregion

#region Vector
        private Vector3? _vectorValue;
        private const short VectorXPosition = 0 * sizeof(float);
        private const short VectorYPosition = 1 * sizeof(float);
        private const short VectorZPosition = 2 * sizeof(float);
        private const short VectorSize = 3 * sizeof(float);
        public Vector3 VectorValue
        {
            get
            {
                if (_vectorValue == null)
                {
                    var x = BitConverter.ToSingle(Value, VectorXPosition);
                    var y = BitConverter.ToSingle(Value, VectorYPosition);
                    var z = BitConverter.ToSingle(Value, VectorZPosition);
                    _vectorValue = new Vector3(x,y,z);
                }
                return _vectorValue.Value;
            }
            set
            {
                SetDataType(DataType.Vector);
                var tmp = new byte[VectorSize];
                var xPos = BitConverter.GetBytes(value.X);
                var yPos = BitConverter.GetBytes(value.Y);
                var zPos = BitConverter.GetBytes(value.Z);

                for (var i = 0; i < sizeof(float); ++i )
                {
                    tmp[VectorXPosition + i] = xPos[i];
                    tmp[VectorYPosition + i] = yPos[i];
                    tmp[VectorZPosition + i] = zPos[i];
                }

                Value = tmp;
            }
        }
#endregion

#region Long
        private long? _longValue;
        public long LongValue
        {
            get
            {
                if (_longValue == null)
                    _longValue = BitConverter.ToInt64(Value, 0);
                return _longValue.Value;
            }
            set
            {
                SetDataType(DataType.Long);
                Value = BitConverter.GetBytes(value);
            }
        }
#endregion

#region Double
        private double? _doubleValue;
        public double DoubleValue
        {
            get
            {
                if (_doubleValue == null)
                    _doubleValue = BitConverter.ToDouble(Value, 0);
                return _doubleValue.Value;
            }
            set
            {
                SetDataType(DataType.Double);
                Value = BitConverter.GetBytes(value);
            }
        }
#endregion

#region Colour
        private Color4? _colourValue;

        public Color4 ColourValue
        {
            get
            {
                if (_colourValue == null)
                    _colourValue = new Color4(BitConverter.ToSingle(Value, 0),
                                              BitConverter.ToSingle(Value, sizeof(float)),
                                              BitConverter.ToSingle(Value, sizeof(float) * 2),
                                              BitConverter.ToSingle(Value, sizeof(float) * 3));
                return _colourValue.Value;
            }
            set
            {
                SetDataType(DataType.Colour);
                Value = BitConverter.GetBytes(value.Alpha).
                    Concat(BitConverter.GetBytes(value.Red)).
                    Concat(BitConverter.GetBytes(value.Green)).
                    Concat(BitConverter.GetBytes(value.Blue)).ToArray();
            }
        }
#endregion

// this uses the c# serialisation, so it's very much not optimised
// it does however mean that we can put any class in here and as long as
// both the client and server have access to that object then it'll all just work(tm).
#region Object
        BinaryFormatter _binaryFormatter;
        private BinaryFormatter Formatter
        {
            get
            {
                if (_binaryFormatter == null)
                    _binaryFormatter = new BinaryFormatter();
                return _binaryFormatter;
            }
        }

        byte[] Serialise(object value)
        {
            var stream = new MemoryStream();
            Formatter.Serialize(stream, value);
            return stream.ToArray();
        }

        object Deserialise(byte[] data)
        {
            var stream = new MemoryStream(data);
            return Formatter.Deserialize(stream);
        }

        private object _objectValue;
        public object ObjectValue
        {
            get
            {
                if (_objectValue == null)
                    _objectValue = Deserialise(Value);
                return _objectValue;
            }
            set
            {
                SetDataType(DataType.Object);
                Value = Serialise(value);
            }
        }
#endregion

        private void Reset()
        {
            _doubleValue = null;
            _boolValue = null;
            _floatValue = null;
            _intValue = null;
            _shortValue = null;
            _longValue = null;
            _stringValue = null;
            _vectorValue = null;
            _colourValue = null;
            _objectValue = null;
        }

            // a mechanism to clear the dirty flag
        public void ClearDirtyFlag()
        {
            IsDirty = false;
        }

        public Trait(short id, byte[] data = null) 
        {
            DataType = DataType.Unassigned;
            IsDirtyable = true;
            Reset();

            if (data == null)
            {
                _value = new byte[]{};
            }
            else
            {
                _value = new byte[data.Length];
                Array.Copy(data, _value, data.Length);
            }
            
            PropertyId = id;
            IsDirty = false;
            IsPersistant = true;
        }

        public Trait(Trait other)
            :this(other.PropertyId, other.ByteArrayValue)
        {
            DataType = other.DataType;
            IsDirtyable = other.IsDirtyable;
        }

        public Trait(short id, object data)
            : this(id)
        {
            ObjectValue = data;
        }

        public Trait(short id, bool data)
            :this(id)
        {
            BoolValue = data;
        }

        public Trait(short id, byte data)
            : this(id)
        {
            ByteValue = data;
        }

        public Trait(short id, int data)
            : this(id)
        {
            IntValue = data;
        }

        public Trait(short id, string data)
            : this(id)
        {
            StringValue = data;
        }

        public Trait(short id, short data)
            : this(id)
        {
            ShortValue = data;
        }

        public Trait(short id, Vector3 data)
            : this(id)
        {
            VectorValue = data;
        }

        public Trait(short id, float data)
            : this(id)
        {
            FloatValue = data;
        }

        public Trait(short id, long data)
            : this(id)
        {
            LongValue = data;
        }

        public Trait(short id, double data)
            : this(id)
        {
            DoubleValue = data;
        }

        public Trait(short id, Color4 data)
            : this(id)
        {
            ColourValue = data;
        }

        protected Trait()
        {
            DataType = DataType.Unassigned;
        }

        public override string ToString()
        {
            switch (DataType)
            {
                case DataType.Float:
                    return FloatValue.ToString(CultureInfo.InvariantCulture);
                case DataType.Long:
                    return LongValue.ToString(CultureInfo.InvariantCulture);
                case DataType.Short:
                    return ShortValue.ToString(CultureInfo.InvariantCulture);
                case DataType.Double:
                    return DoubleValue.ToString(CultureInfo.InvariantCulture);
                case DataType.Colour:
                    return ColourValue.ToString();
                case DataType.Vector:
                    return VectorValue.ToString();
                case DataType.Byte:
                    return ByteValue.ToString(CultureInfo.InvariantCulture);
                case DataType.Bool:
                    return BoolValue.ToString(CultureInfo.InvariantCulture);
                case DataType.Int:
                    return IntValue.ToString(CultureInfo.InvariantCulture);
                case DataType.String:
                    return StringValue;
                case DataType.ByteArray:
                    return BitConverter.ToString(Value);
                case DataType.Object:
                    return ObjectValue.ToString();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    public enum DataType
    {
        Unassigned = 0,
        Float,
        Long,
        Short,
        Double,
        Colour,
        Vector,
        Byte,
        ByteArray,
        Bool,
        Int,
        String,
        Object
    }
}
