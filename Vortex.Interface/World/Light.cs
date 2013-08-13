using System;
using Psy.Core;
using SlimMath;

namespace Vortex.Interface.World
{
    public class Light : ILight
    {
        private Vector3 _position;
        private float _brightness;
        private Color4 _colour;
        private bool _enabled;

        public Vector3 Position
        {
            get { return _position; }
            set
            {
                if (!_position.Equals(value))
                    IsDirty = true;
                _position = value;
            }
        }

        public float Brightness
        {
            get { return _brightness; }
            set
            {
                if (Math.Abs(_brightness - value) > 0.001f)
                    IsDirty = true;
                _brightness = value;
            }
        }

        public bool Enabled
        {
            get { return _enabled; }
            set
            {
                if (_enabled != value)
                    IsDirty = true;
                _enabled = value;
            }
        }

        public Color4 Colour
        {
            get
            {
                return _colour;
            }
            set
            {
                if (!_colour.Equals(value))
                    IsDirty = true;
                _colour = value;
            }
        }

        public bool IsDirty { get; set; }
        public bool IsDynamic { get; set; }

        public ILight Translate(Vector3 amount)
        {
            return new Light(Position + amount, Brightness, Colour);
        }

        public void Checked()
        {
            IsDirty = false;
        }

        public Light()
        {
            Position = new Vector3();
            Colour = new Color4(1.0f, 1.0f, 1.0f, 1.0f);
            Brightness = 2.0f;
            IsDynamic = false;
        }

        public Light(Vector3 position, float brightness, Color4 colour)
        {
            Position = position;
            Brightness = brightness;
            Colour = colour;
            IsDirty = false;
            Enabled = true;
        }

        public Color4 ColorAt(Vector3 samplePoint)
        {
            var distance = samplePoint.Distance(Position);
            var multiplier = ((1/distance)*Brightness) - 1;

            if (multiplier > 1.0f)
                multiplier = 1.0f;

            var result = Colour*multiplier;
            return result;
        }
    }
}
