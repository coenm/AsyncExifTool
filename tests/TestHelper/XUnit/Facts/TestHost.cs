namespace TestHelper.XUnit.Facts
{
    using System;

    [Flags]
    public enum TestHost
    {
        /// <summary>
        /// Everything but a continuous integration system.
        /// </summary>
        Local = 0x01,

        /// <summary>
        /// Appveyor windows virtual machine.
        /// </summary>
        AppVeyorWindows = 0x01 << 1,

        /// <summary>
        /// Appveyor Linux virtual machine.
        /// </summary>
        AppVeyorLinux = 0x01 << 2,

        /// <summary>
        /// Appveyor (Linux or Windows) virtual machine
        /// </summary>
        AppVeyor = AppVeyorWindows | AppVeyorLinux,

        /// <summary>
        /// Travis virtual machine.
        /// </summary>
        Travis = 0x01 << 3,

        /// <summary>
        /// AzureDevops
        /// </summary>
        AzureDevopsWindows = 0x01 << 4,
    }
}
