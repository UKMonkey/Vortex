namespace Vortex.Renderer
{
    internal class RenderResult : IRenderResult
    {
        public int VertexRenderCount { get; set; }
        public int StateChanges { get; set; }

        public void Reset()
        {
            VertexRenderCount = 0;
            StateChanges = 0;
        }

        /// <summary>
        /// Combines totals from another result, updating
        /// this instances values in-place.
        /// </summary>
        /// <param name="other"></param>
        public void CombineResultFrom(RenderResult other)
        {
            StateChanges += other.StateChanges;
            VertexRenderCount += other.VertexRenderCount;
        }

        public void IncrementStateChange(int amount=1)
        {
            StateChanges += amount;
        }
    }
}