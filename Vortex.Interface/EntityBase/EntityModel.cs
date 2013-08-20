using Psy.Graphics.Models;
using System.Diagnostics;

namespace Vortex.Interface.EntityBase
{
    public class EntityModel
    {
        private readonly CompiledModelCache _compiledModelCache;
        public ModelInstance ModelInstance;

        public EntityModel(CompiledModelCache compiledModelCache)
        {
            _compiledModelCache = compiledModelCache;
        }

        public void SetModel(string filename)
        {
            ModelInstance = _compiledModelCache.GetModel(filename);
            Debug.Assert(ModelInstance != null);
            ModelInstance.RemoveSubModels();
        }
    }
}