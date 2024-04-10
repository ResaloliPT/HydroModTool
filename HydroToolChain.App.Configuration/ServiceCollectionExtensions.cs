using HydroToolChain.App.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static void ConfigureWritable<T>(  
        this IServiceCollection services,
        string file, bool optional = true, string? fileRoot = null) where T : class, new()
    {
        services.AddOptions();

        var filePath = fileRoot is null ? file : Path.Combine(fileRoot, file);
        
        var configurationBuilder = new ConfigurationBuilder()
            .AddJsonFile(file, optional);

        IConfigurationRoot configuration;
        try
        {
            configuration = configurationBuilder
                .Build();
        }
        catch (FileNotFoundException)
        {
            CreateDefaultFile<T>(filePath, false);
            
            configuration = configurationBuilder
                .Build();
        }
        catch (InvalidDataException)
        {
            var date = DateTime.Now;
            
            File.Copy(filePath, $"{filePath}_{date:yyyyMMddHHmmss}.bak");
            
            CreateDefaultFile<T>(filePath, true);
            
            configuration = configurationBuilder
                .Build();
        }

        services.Configure<T>(configuration);
        services.AddTransient<IWritableOptions<T>>(provider =>  
        {  
            var options = provider.GetRequiredService<IOptionsMonitor<T>>();  
            return new WritableOptions<T>(options, configuration, file);  
        });
    }

    private static void CreateDefaultFile<T>(string filePath, bool truncate) where T : class, new()
    {
        using var fileHandle = File.Open(filePath, truncate ? FileMode.Truncate : FileMode.OpenOrCreate);
        using var fileWriter = new StreamWriter(fileHandle);
        fileWriter.Write(JsonConvert.SerializeObject(new T(), Formatting.Indented));
        fileWriter.Flush();
        fileWriter.Close();
    }
}