namespace IdentityMicroService
{
    public interface IEndpoint<T>
    {
        void AddRoute(IEndpointRouteBuilder app);


    }
}
