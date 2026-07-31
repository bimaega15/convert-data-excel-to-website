namespace Sifp_Vue.Server.Models.Dtos
{
    /// <summary>
    /// Amplop respons seragam untuk seluruh endpoint /api (mengikuti pola Urbuddy).
    /// Klien cukup memeriksa <see cref="Status"/> tanpa menebak bentuk body per endpoint.
    /// </summary>
    public class ApiResponse<T>
    {
        public string Status { get; set; } = ApiStatus.Success;
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }

        /// <summary>Detail validasi per field. Null bila tidak ada error validasi.</summary>
        public IDictionary<string, string[]>? Errors { get; set; }

        public static ApiResponse<T> Ok(T data, string message = "OK") => new()
        {
            Status = ApiStatus.Success,
            Message = message,
            Data = data
        };

        public static ApiResponse<T> Fail(string message, IDictionary<string, string[]>? errors = null) => new()
        {
            Status = ApiStatus.Error,
            Message = message,
            Data = default,
            Errors = errors
        };
    }

    public static class ApiStatus
    {
        public const string Success = "SUCCESS";
        public const string Error = "ERROR";
    }

    /// <summary>Hasil query berhalaman. Dipakai semua endpoint list master data.</summary>
    public class PagedResult<T>
    {
        public IReadOnlyList<T> Items { get; set; } = Array.Empty<T>();
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalItems / (double)PageSize);
        public bool HasPrevious => Page > 1;
        public bool HasNext => Page < TotalPages;
    }

    /// <summary>Parameter query standar: paging, pencarian, dan pengurutan.</summary>
    public class QueryParameters
    {
        private const int MaxPageSize = 200;
        private int _pageSize = 25;
        private int _page = 1;

        public int Page
        {
            get => _page;
            set => _page = value < 1 ? 1 : value;
        }

        /// <summary>Dibatasi maksimum 200 supaya satu request tidak menarik seluruh tabel.</summary>
        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value switch
            {
                < 1 => 25,
                > MaxPageSize => MaxPageSize,
                _ => value
            };
        }

        /// <summary>Kata kunci pencarian bebas; tiap repository menentukan kolom yang dicari.</summary>
        public string? Search { get; set; }

        public string? SortBy { get; set; }
        public bool SortDescending { get; set; }
    }
}
