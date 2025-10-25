using System.ComponentModel;

namespace ToDoListCore.Enums
{
    public enum HostType
    {
        [Description("Visual Studio")]
        VisualStudio,
        [Description("IIS")]
        IIS,
        [Description("Kestrel")]
        Kestrel,
        [Description("Docker Container")]
        DockerContainer
    }
}
