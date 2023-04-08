﻿using System.Diagnostics;

if (args.Length == 0)
{
    Console.WriteLine("insufficient params. pleae refer to https://fullstackhero.net/dotnet-webapi-boilerplate/general/fsh-api-console");
    return;
}

string firstArg = args[0];
if (firstArg == "install" || firstArg == "i")
{
    await InstallTemplates();
    return;
}

if (firstArg == "update" || firstArg == "u")
{
    await UpdateFshCliTool();
    return;
}

async Task UpdateFshCliTool()
{
    Console.WriteLine("updating the fsh cli tool...");
    var fshPsi = new ProcessStartInfo
    {
        FileName = "dotnet",
        Arguments = "tool update FSH.CLI --global -v=q"
    };
    using var fshProc = Process.Start(fshPsi)!;
    await fshProc.WaitForExitAsync();

    Console.WriteLine("completed updating the fsh cli tool.");
    await InstallTemplates();
    Console.WriteLine("completed updating the fsh templates.");
}

if (firstArg == "api")
{
    if (args.Length != 3)
    {
        Console.WriteLine("invalid command. use something like : fsh api new <projectname>");
        return;
    }

    string command = args[1];
    string projectName = args[2];
    if (command == "n" || command == "new")
    {
        await BootstrapWebApiSolution(projectName);
    }

    return;
}

if (firstArg == "wasm")
{
    if (args.Length != 3)
    {
        Console.WriteLine("invalid command. use something like : fsh wasm new <projectname>");
        return;
    }

    string command = args[1];
    string projectName = args[2];
    if (command == "n" || command == "new")
    {
        await BootstrapBlazorWasmSolution(projectName);
    }

    return;
}

async Task InstallTemplates()
{
    Console.WriteLine("installing fsh dotnet webapi template...");
    var apiPsi = new ProcessStartInfo
    {
        FileName = "dotnet",
        Arguments = "new install FullStackHero.WebAPI.Boilerplate -v=q"
    };
    using var apiProc = Process.Start(apiPsi)!;
    await apiProc.WaitForExitAsync();

    Console.WriteLine("installing fsh blazor wasm template...");
    var wasmPsi = new ProcessStartInfo
    {
        FileName = "dotnet",
        Arguments = "new install FullStackHero.BlazorWebAssembly.Boilerplate -v=q"
    };
    using var wasmProc = Process.Start(wasmPsi)!;
    await wasmProc.WaitForExitAsync();

    Console.WriteLine("installed the required templates.");
    Console.WriteLine("get started by using : fsh <type> new <projectname>.");
    Console.WriteLine("note : <type> can be api, wasm.");
    Console.WriteLine("refer to documentation at https://fullstackhero.net/dotnet-webapi-boilerplate/general/getting-started");
}

async Task BootstrapWebApiSolution(string projectName)
{
    Console.WriteLine($"bootstraping fullstackhero dotnet webapi project for you at \"./{projectName}\"...");
    var psi = new ProcessStartInfo
    {
        FileName = "dotnet",
        Arguments = $"new fsh-api -n {projectName} -o {projectName} -v=q"
    };
    using var proc = Process.Start(psi)!;
    await proc.WaitForExitAsync();
    Console.WriteLine($"fsh-api project {projectName} successfully created.");
    Console.WriteLine("application ready! build something amazing!");
    Console.WriteLine("refer to documentation at https://fullstackhero.net/dotnet-webapi-boilerplate/general/getting-started");
}

async Task BootstrapBlazorWasmSolution(string projectName)
{
    Console.WriteLine($"bootstraping fullstackhero blazor wasm solution for you at \"./{projectName}\"...");
    var psi = new ProcessStartInfo
    {
        FileName = "dotnet",
        Arguments = $"new fsh-blazor -n {projectName} -o {projectName} -v=q"
    };
    using var proc = Process.Start(psi)!;
    await proc.WaitForExitAsync();
    Console.WriteLine($"fullstackhero blazor wasm solution {projectName} successfully created.");
    Console.WriteLine("application ready! build something amazing!");
    Console.WriteLine("refer to documentation at https://fullstackhero.net/blazor-webassembly-boilerplate/general/getting-started/");
}