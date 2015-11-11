namespace Orient.Client.API.Query.Interfaces
{
    public interface IOCreateCluster
    {
        IOCreateCluster Cluster(string clusterName, OClusterType clusterType);
        IOCreateCluster Cluster<T>(OClusterType clusterType);
        short Run();
        string ToString();
    }
}
