namespace HNTAS.Web.UI.Helpers
{
    public interface ISessionHelper
    {
        void SaveToSession<T>(HttpContext httpContext, string sessionKey, T model);
        T? GetFromSession<T>(HttpContext httpContext, string sessionKey) where T : class;
        void ClearFromSession(HttpContext httpContext, string sessionKey);
        void ClearAllFlowRelatedSessionData(HttpContext context);
        void SetIsCheckAnswerFlow(HttpContext httpContext, bool isCheckAnswerFlow);
        bool GetIsCheckAnswerFlow(HttpContext httpContext);
    }
}
