namespace ExifToolAsync.ExifTool
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IExifTool
    {
        void Init();

        Task<string> ExecuteAsync(IEnumerable<string> args, CancellationToken ct = default);

        Task DisposeAsync(CancellationToken ct = default);
    }
}