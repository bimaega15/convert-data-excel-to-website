namespace Sifp_Vue.Server.Models.Entities
{
    /// <summary>
    /// Ditandai pada entitas yang selalu menjadi anak sebuah observasi.
    /// Memungkinkan satu repository generik menangani filter "per observasi"
    /// untuk SIF question, error trap, HP tool, drift, dan latent condition.
    /// </summary>
    public interface IObservationChild
    {
        int Id { get; }
        int ObservationId { get; set; }
        Observation? Observation { get; set; }
        string? ProtocolCode { get; set; }
        int? ImportBatchId { get; set; }
    }
}
