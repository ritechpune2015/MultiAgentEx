using MultiAgentEx.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddScoped<AgentFactory>();
builder.Services.AddScoped<MultiAgentOrchestrator>();
builder.Services.AddControllers();
builder.Services.AddSwaggerGen();
builder.Services.AddEndpointsApiExplorer();
var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();
app.Run();
