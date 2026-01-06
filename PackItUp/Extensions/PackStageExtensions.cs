using Modrinth.Models.Enums.Project;
using PackItUp.Types;

namespace PackItUp.Extensions;

public static class PackStageExtensions
{
    public static ProjectVersionType GetModrinth(this PackStage stage)
        => stage switch
        {
            PackStage.Release => ProjectVersionType.Release,
            PackStage.Beta    => ProjectVersionType.Beta,
            PackStage.Alpha   => ProjectVersionType.Alpha,
            _                 => ProjectVersionType.Release
        };
}