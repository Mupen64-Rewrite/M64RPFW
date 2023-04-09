using M64RPFW.Models.Types;

namespace M64RPFW.Services.Abstractions;

public record OpenMovieDialogResult(string Path, string Authors, string Description, Mupen64PlusTypes.VCRStartType StartType);