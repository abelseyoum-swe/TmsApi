public class EnrollmentWorker(IServiceScopeFactory scopeFactory)
{
    public void ProcessBatch()
    {
        // TODO 2: Create a short-lived scope
        using var scope = scopeFactory.CreateScope();
        
        // TODO 3: Resolve the scoped service from the new scope's provider
        var svc = scope.ServiceProvider.GetRequiredService<IEnrollmentService>();
        
        // TODO 4: Use the service, then let the 'using' block dispose the scope
        var enrollments = svc.GetAllAsync().Result;
        // ... process enrollments
    }
}