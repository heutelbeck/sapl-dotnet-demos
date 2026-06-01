using Sapl.AspNetCore.Extensions;
using Sapl.Core.Pep.Constraints;
using Sapl.Demo.Handlers;
using Sapl.Demo.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase);

builder.Services.AddSapl(options =>
    options.BaseUrl = builder.Configuration.GetValue("SAPL_PDP_URL", "http://localhost:8443")!);

builder.Services.AddSaplConstraintHandler<LogAccessHandler>();
builder.Services.AddSaplConstraintHandler<RedactFieldsHandler>();
builder.Services.AddSaplConstraintHandler<ClassificationFilterHandler>();
builder.Services.AddSaplConstraintHandler<InjectTimestampHandler>();
builder.Services.AddSaplConstraintHandler<CapTransferHandler>();
builder.Services.AddSaplConstraintHandler<NotifyOnErrorHandler>();
builder.Services.AddSaplConstraintHandler<EnrichErrorHandler>();

builder.Services.AddSingleton<AuditTrailHandler>();
builder.Services.AddSingleton<IConstraintHandlerProvider>(sp => sp.GetRequiredService<AuditTrailHandler>());

// Domain-level enforcement: IPatientService methods carry the attributes and are
// intercepted by the DispatchProxy. Streaming stays controller-level (StreamingController
// carries [StreamEnforce]), so IStreamingService is a plain registration.
builder.Services.AddSaplService<IPatientService, PatientService>();
builder.Services.AddSingleton<IStreamingService, StreamingService>();

var app = builder.Build();

app.UseSaplAccessDenied();
app.MapControllers();

var port = builder.Configuration.GetValue("PORT", 3000);
app.Run($"http://0.0.0.0:{port}");
