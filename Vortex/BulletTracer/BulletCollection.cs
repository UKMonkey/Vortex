using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Vortex.BulletTracer
{
    public class BulletCollection : Collection<Bullet>
    {
        private readonly List<Bullet> _deadBullets; 

        public BulletCollection()
        {
            _deadBullets = new List<Bullet>();
        }

        public void Update()
        {
            _deadBullets.Clear();
            foreach (var bullet in Items.Where(bullet => !bullet.Update()))
            {
                _deadBullets.Add(bullet);
            }

            foreach (var deadBullet in _deadBullets)
            {
                Items.Remove(deadBullet);
            }
        }
    }
}
