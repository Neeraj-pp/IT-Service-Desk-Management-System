using HelpDesk.DAL.Model;

namespace HelpDesk.DAL.Repository
{
    public interface IRepository
    {
       
        bool Authenticate(string userName, string password);

        bool RegisterUser(string userName, string password);

        List<ServiceRequest> ViewRequests();

        List<ServiceRequest> ViewRequests(string userName);

        int RaiseRequest(ServiceRequest newRequest);

        ServiceRequest? GetRequestById(int requestId);

        bool ReOpenRequest(ServiceRequest request);

        List<ServiceRequest> GetRequestBySP(string userName);

        bool CloseRequest(int requestId);

        bool DeleteRequest(int requestId);

        User? GetUser(string userName);

        (List<ServiceRequest> Items, int TotalCount)
    GetRequestsPaged(int pageNumber, int pageSize);

        (List<ServiceRequest> Items, int TotalCount)
    GetRequestsPaged(
        int pageNumber,
        int pageSize,
        string? searchTerm,
        int? statusId,
        DateTime? fromDate,
        DateTime? toDate,
        string? userName
    );


        (int Total, int New, int Reopened, int Closed)
    GetRequestStatusCounts();
    }


}