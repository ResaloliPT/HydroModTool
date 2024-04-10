using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace HydroToolChain.App.Configuration;

internal class WritableOptions<T> : IWritableOptions<T> where T : class, new()  
{
    private readonly IOptionsMonitor<T> _options;  
    private readonly FileInfo _file;
    private readonly IConfigurationRoot _configuration;

    public WritableOptions(  
        IOptionsMonitor<T> options,  
        IConfigurationRoot configuration,  
        string file)  
    {  
        _options = options;  
        _configuration = configuration;  
        _file = new FileInfo(file);
    }
    
    public T Value => _options.CurrentValue;

    public void RequestUpdate()
    {
        _configuration.Reload();
    }

    public T GetValue()
    {
        return _options.CurrentValue;
    }

    public async Task Update(Action<T> applyChanges)
    {
        await Update(applyChanges, CancellationToken.None);
    }

    public async Task Update(Action<T> applyChanges, CancellationToken ct)
    {
        var sectionObject = new T();

        await using (var file = File.Open(_file.FullName, FileMode.OpenOrCreate))
        using (var reader = new StreamReader(file))
        {
            var jObject = JsonConvert.DeserializeObject<T>(
                await reader.ReadToEndAsync(),
                new JsonSerializerSettings
                {
                    MissingMemberHandling = MissingMemberHandling.Error,
                    ObjectCreationHandling = ObjectCreationHandling.Replace,
                });
            
            if (jObject != null)
            {
                sectionObject = jObject;
            }
        }
        
        await using (var file = File.Open(_file.FullName, FileMode.Truncate))
        await using (var writer = new StreamWriter(file))
        {
            applyChanges(sectionObject);
            
            await writer.WriteAsync(JsonConvert.SerializeObject(sectionObject, Formatting.Indented));

            await writer.FlushAsync();
            
            writer.Close();
        }
        
        _configuration.Reload();
    }
}