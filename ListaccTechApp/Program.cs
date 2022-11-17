using ListaccTechApp.Interfaces;
using ListaccTechApp.Models;
using ListaccTechApp.Models.Data;
using ListaccTechApp.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using System.Data;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers().AddNewtonsoftJson(opt => {
    opt.SerializerSettings.ReferenceLoopHandling =
    Newtonsoft.Json.ReferenceLoopHandling.Ignore;
    // opt.SerializerSettings.Converters.Add(new TrimmingConverter());
});

// Adding Dependency Injection

builder.Services.AddScoped<ITokenGenerator, TokenGenerator>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IOtherServices, OtherServices>();
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<ILearningPathService, LearningPathService>();
builder.Services.AddScoped<IModuleService, ModuleService>();
builder.Services.AddScoped<ITopicService, TopicService>();
builder.Services.AddScoped<ILessonService, LessonService>();


//Db context

builder.Services.AddDbContext<MyDbContext>(option => option.UseNpgsql(
    builder.Configuration.GetConnectionString("DefaultConnection")
));

//AutoMapper
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

//for Identity
IdentityBuilder builder1 = builder.Services.AddIdentityCore<User>(opt => {
    opt.Password.RequireDigit = false;
    opt.Password.RequiredLength = 6;
    opt.Password.RequireNonAlphanumeric = false;
    opt.Password.RequireUppercase = false;
});
builder1 = new IdentityBuilder(builder1.UserType, typeof(Role), builder.Services);
builder1.AddEntityFrameworkStores<MyDbContext>();
builder1.AddRoleValidator<RoleValidator<Role>>();
builder1.AddDefaultTokenProviders();

builder.Services.AddHttpClient();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options => {

                    options.SaveToken = true;
                    options.RequireHttpsMetadata = false;

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Tokens:Key"])),
                        ClockSkew = TimeSpan.Zero,
                    };
                });


// Learn more about configuring Swagger/OpenListaccTechApp at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

//for swagger to use jwt
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Description = "Standard Authorization header using the Bearer scheme (\"Bearer {token}\")",
        In = ParameterLocation.Header,
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });
    options.OperationFilter<SecurityRequirementsOperationFilter>();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});


app.MapControllers();

app.Run();
