namespace Orient.Client
{
    public interface IBaseRecord
    {
        Orid Orid { get; set; }
        int OVersion { get; set; }
        short OClassId { get; set; }
        string OClassName { get; set; }
    }
}