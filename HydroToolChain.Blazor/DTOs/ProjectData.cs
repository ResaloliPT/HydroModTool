using System.ComponentModel.DataAnnotations;
using HydroToolChain.Blazor.State;

namespace HydroToolChain.Blazor.DTOs;

public class ProjectData
{
    public ProjectData(){}

    public ProjectData(ProjectState state)
    {
        Id = state.Id;
        Name = state.Name;
        ModIndex = state.ModIndex;
        CookedAssetsPath = state.CookedAssetsPath;
        OutputPath = state.OutputPath;
    }
    
    public Guid Id { get; } = Guid.NewGuid();
    
    [Required(AllowEmptyStrings = false, ErrorMessage = "Name is Required")]
    [MinLength(3, ErrorMessage = "Must be at least 3 characters")]
    public string Name { get; set; } = "";

    [Required(AllowEmptyStrings = false, ErrorMessage = "Index is Required")]
    public int ModIndex { get; set; } = 500;

    [Required(AllowEmptyStrings = false, ErrorMessage = "Asset Path is Required")]
    [CustomValidation(typeof(ProjectData), nameof(ValidatePath))]

    public string CookedAssetsPath { get; set; } = "";
    
    [Required(AllowEmptyStrings = false, ErrorMessage = "Dist Path is Required")]
    [CustomValidation(typeof(ProjectData), nameof(ValidatePath))]

    public string OutputPath { get; set; } = "";
    
    public static ValidationResult? ValidatePath(string path, ValidationContext vctx)
    {
        try
        {
            if (!Directory.Exists(path))
            {
                return new ValidationResult("Path doesn't exist");
            }
        }
        catch (Exception)
        {
            return new ValidationResult("Invalid path or can´t write to path");
        }
        
        return ValidationResult.Success;
    }
}