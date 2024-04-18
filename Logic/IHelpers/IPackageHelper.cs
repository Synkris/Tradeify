using Core.Models;


namespace Logic.IHelpers
{
    public interface IPackageHelper
    {
        List<News> GetNews();
        List<News> GetAllNews();
        List<Packages> GetPackages();
        Task<Packages> GetPackageById(int packageId);


    }
}
