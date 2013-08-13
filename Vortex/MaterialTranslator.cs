using Psy.Core;
using Psy.Core.EpicModel.Serialization;

namespace Vortex
{
    public class MaterialTranslator : IMaterialTranslator
    {
        private readonly MaterialCache _materialCache;

        public MaterialTranslator(MaterialCache materialCache)
        {
            _materialCache = materialCache;
        }

        public string Translate(int materialId)
        {
            return _materialCache[materialId].TextureName;
        }

        public int Translate(string textureName)
        {
            return _materialCache.GetByTextureName(textureName).Id;
        }
    }
}