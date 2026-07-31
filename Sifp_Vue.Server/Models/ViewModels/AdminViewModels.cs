using Sifp_Vue.Server.Models.Dtos;

namespace Sifp_Vue.Server.Models.ViewModels
{
    public class AdminDashboardViewModel
    {
        public AdminDashboardSummaryDto Summary { get; set; } = new();
        public IReadOnlyDictionary<string, int> RowCounts { get; set; } = new Dictionary<string, int>();
        public IReadOnlyList<ImportBatchDto> RecentImports { get; set; } = Array.Empty<ImportBatchDto>();
    }

    /// <summary>
    /// Pembungkus daftar untuk halaman Razor: menyatukan hasil paging dengan
    /// nilai filter yang sedang aktif, supaya form filter bisa dirender ulang.
    /// </summary>
    public class ListViewModel<TItem, TQuery> where TQuery : QueryParameters, new()
    {
        public PagedResult<TItem> Result { get; set; } = new();
        public TQuery Query { get; set; } = new();

        public string Title { get; set; } = string.Empty;
        public string? Subtitle { get; set; }
    }

    public class ObservationListViewModel : ListViewModel<ObservationDto, ObservationQuery>
    {
        public ObservationFilterOptionsDtoWrapper Options { get; set; } = new();
    }

    /// <summary>Nilai dropdown filter observasi. Dipisah agar view tidak menyentuh service.</summary>
    public class ObservationFilterOptionsDtoWrapper
    {
        public List<int> Zonas { get; set; } = new();
        public List<string> ProtocolCodes { get; set; } = new();
        public List<string> Sites { get; set; } = new();
        public List<string> Companies { get; set; } = new();
        public List<string> Statuses { get; set; } = new();
    }

    public class InitiativeListViewModel : ListViewModel<InitiativeDto, InitiativeQuery>
    {
        public List<string> Statuses { get; set; } = new();
    }

    public class UserListViewModel : ListViewModel<UserDto, QueryParameters>
    {
    }

    public class ImportBatchListViewModel : ListViewModel<ImportBatchDto, QueryParameters>
    {
    }

    /// <summary>Form create/edit observasi beserta konteks tampilannya.</summary>
    public class ObservationFormViewModel
    {
        public int? Id { get; set; }
        public ObservationRequest Form { get; set; } = new();
        public bool IsEdit => Id.HasValue;
        public string PageTitle => IsEdit ? "Edit Observasi" : "Tambah Observasi";
    }

    public class InitiativeFormViewModel
    {
        public int? Id { get; set; }
        public InitiativeRequest Form { get; set; } = new();
        public bool IsEdit => Id.HasValue;
        public string PageTitle => IsEdit ? "Edit Inisiatif" : "Tambah Inisiatif";
    }

    public class UserFormViewModel
    {
        public int? Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public CreateUserRequest CreateForm { get; set; } = new();
        public UpdateUserRequest EditForm { get; set; } = new();
        public IReadOnlyList<RoleDto> AvailableRoles { get; set; } = Array.Empty<RoleDto>();
        public bool IsEdit => Id.HasValue;
        public string PageTitle => IsEdit ? "Edit User" : "Tambah User";
    }

    /// <summary>
    /// Satu blok tabel turunan pada halaman detail observasi. Dipakai lewat partial
    /// (bukan local function di Razor) karena partial memuat tag helper dan harus async.
    /// </summary>
    public class ChildTableViewModel
    {
        public string Title { get; set; } = string.Empty;
        public List<string> Headers { get; set; } = new();
        public List<string?[]> Rows { get; set; } = new();
    }

    /// <summary>Halaman daftar generik untuk master data yang hanya bisa dibaca.</summary>
    public class MasterDataTableViewModel
    {
        public string Title { get; set; } = string.Empty;
        public string? Subtitle { get; set; }
        public string RouteName { get; set; } = string.Empty;
        public List<string> Columns { get; set; } = new();
        public List<List<string?>> Rows { get; set; } = new();
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 25;
        public int TotalItems { get; set; }
        public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalItems / (double)PageSize);
        public string? Search { get; set; }
        public string? ObsCode { get; set; }
    }
}
