namespace Roux.StandardLibrary
{
    public sealed class RouxStandardLibrary : IRouxLibraryBinder
    {
        public void Bind(RouxRuntime runtime)
        {
            runtime.DefineValue("List", new ListClass());
            runtime.DefineValue("Map", new MapClass());
        }
    }
}