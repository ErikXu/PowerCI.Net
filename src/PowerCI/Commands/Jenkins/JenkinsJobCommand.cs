using McMaster.Extensions.CommandLineUtils;
using Newtonsoft.Json;
using RestSharp;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace PowerCI.Commands.Jenkins
{
    [Command("job", Description = "Handle jenkins jobs"), 
     Subcommand(typeof(JenkinsJobListCommand)),
     Subcommand(typeof(JenkinsJobCreateCommand)),
     Subcommand(typeof(JenkinsJobCopyCommand))]
    internal class JenkinsJobCommand
    {
        public void OnExecute(IConsole console)
        {
            console.WriteLine("Handle jenkins jobs");
        }
    }

    [Command("list", Description = "List jenkins jobs")]
    internal class JenkinsJobListCommand : JenkinsParameter
    {
        [Option(Description = "Folder Name", ShortName = "f")]
        public string Folder { get; set; }

        public void OnExecute(IConsole console, IJenkinsService jenkinsService)
        {
            var (isValid, host, user, tokenOrPassword) = ValidParameters(console);
            if (!isValid)
            {
                return;
            }

            var (success, list) = jenkinsService.ListJob(host, user, tokenOrPassword, Folder ?? string.Empty);
            if (!success)
            {
                return;
            }

            console.WriteLine(JsonConvert.SerializeObject(list, Formatting.Indented));
        }
    }

    [Command("create", Description = "Create jenkins job")]
    internal class JenkinsJobCreateCommand : JenkinsParameter
    {
        [Option(Description = "Folder Name", ShortName = "f")]
        public string Folder { get; set; }

        [Option(Description = "Job Name", ShortName = "n")]
        [Required]
        public string Name { get; set; }

        [Option(Description = "Job Type - freestyle, pipeline, multibranch", ShortName = "t")]
        public string Type { get; set; }

        private readonly string _freestyle = @"<project>
<description/>
<keepDependencies>false</keepDependencies>
<properties>
<com.dabsquared.gitlabjenkins.connection.GitLabConnectionProperty>
<gitLabConnection/>
<jobCredentialId/>
<useAlternativeCredential>false</useAlternativeCredential>
</com.dabsquared.gitlabjenkins.connection.GitLabConnectionProperty>
</properties>
<scm class=""hudson.scm.NullSCM""/>
<canRoam>true</canRoam>
<disabled>false</disabled>
<blockBuildWhenDownstreamBuilding>false</blockBuildWhenDownstreamBuilding>
<blockBuildWhenUpstreamBuilding>false</blockBuildWhenUpstreamBuilding>
<triggers/>
<concurrentBuild>false</concurrentBuild>
<builders/>
<publishers/>
<buildWrappers/>
</project>";

        private readonly string _pipeline = @"<flow-definition>
<description/>
<keepDependencies>false</keepDependencies>
<properties>
<com.dabsquared.gitlabjenkins.connection.GitLabConnectionProperty>
<gitLabConnection/>
<jobCredentialId/>
<useAlternativeCredential>false</useAlternativeCredential>
</com.dabsquared.gitlabjenkins.connection.GitLabConnectionProperty>
</properties>
<definition class=""org.jenkinsci.plugins.workflow.cps.CpsFlowDefinition"">
<script/>
<sandbox>true</sandbox>
</definition>
<triggers/>
<disabled>false</disabled>
</flow-definition>";

        private readonly string _multibranch = @"<org.jenkinsci.plugins.workflow.multibranch.WorkflowMultiBranchProject>
<properties/>
<folderViews class=""jenkins.branch.MultiBranchProjectViewHolder"">
<owner class=""org.jenkinsci.plugins.workflow.multibranch.WorkflowMultiBranchProject"" reference=""../..""/>
</folderViews>
<healthMetrics/>
<icon class=""jenkins.branch.MetadataActionFolderIcon"">
<owner class=""org.jenkinsci.plugins.workflow.multibranch.WorkflowMultiBranchProject"" reference=""../..""/>
</icon>
<orphanedItemStrategy class=""com.cloudbees.hudson.plugins.folder.computed.DefaultOrphanedItemStrategy"">
<pruneDeadBranches>true</pruneDeadBranches>
<daysToKeep>-1</daysToKeep>
<numToKeep>-1</numToKeep>
<abortBuilds>false</abortBuilds>
</orphanedItemStrategy>
<triggers/>
<disabled>false</disabled>
<sources class=""jenkins.branch.MultiBranchProject$BranchSourceList"">
<data/>
<owner class=""org.jenkinsci.plugins.workflow.multibranch.WorkflowMultiBranchProject"" reference=""../..""/>
</sources>
<factory class=""org.jenkinsci.plugins.workflow.multibranch.WorkflowBranchProjectFactory"">
<owner class=""org.jenkinsci.plugins.workflow.multibranch.WorkflowMultiBranchProject"" reference=""../..""/>
<scriptPath>Jenkinsfile</scriptPath>
</factory>
</org.jenkinsci.plugins.workflow.multibranch.WorkflowMultiBranchProject>";

        public void OnExecute(IConsole console, IJenkinsService jenkinsService)
        {
            var (isValid, host, user, tokenOrPassword) = ValidParameters(console);
            if (!isValid)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(Name))
            {
                console.WriteLine("Name is required");
                return;
            }

            var nameWithFolder = !string.IsNullOrWhiteSpace(Folder) ? $"{Folder}/{Name}" : Name;
            using var client = new RestClient();
            bool success;
            bool isExisted;
            JenkinsCrumb crumb;

            if (!string.IsNullOrWhiteSpace(Folder))
            {
                (success, var list) = jenkinsService.ListFolder(host, user, tokenOrPassword);
                if (!success)
                {
                    return;
                }

                isExisted = list.Any(n => (n.Name ?? string.Empty).Equals(Folder, StringComparison.OrdinalIgnoreCase));

                if (!isExisted)
                {
                    (success, isExisted) = jenkinsService.CheckExists(host, user, tokenOrPassword, string.Empty, Folder);
                    if (!success)
                    {
                        return;
                    }

                    if (isExisted)
                    {
                        console.WriteLine($"Name [{Folder}] exists");
                        return;
                    }

                    (success, crumb) = jenkinsService.GetCrumb(client, host, user, tokenOrPassword);
                    if (!success || crumb == null)
                    {
                        return;
                    }

                    success = jenkinsService.CreateFolder(client, host, user, tokenOrPassword, Folder, crumb);
                    if (!success)
                    {
                        return;
                    }
                }
            }

            (success, isExisted) = jenkinsService.CheckExists(host, user, tokenOrPassword, Folder ?? string.Empty, Name);
            if (!success)
            {
                return;
            }

            if (isExisted)
            {
                console.WriteLine($"Name [{nameWithFolder}] exists");
                return;
            }

            (success, crumb) = jenkinsService.GetCrumb(client, host, user, tokenOrPassword);
            if (!success || crumb == null)
            {
                return;
            }

            var content = _freestyle;
            if (Type == "pipeline")
            {
                content = _pipeline;
            }
            else if (Type == "multibranch")
            {
                content = _multibranch;
            }

            success = jenkinsService.CreateJob(client, host, user, tokenOrPassword, Folder ?? string.Empty, Name, crumb, content);
            if (!success)
            {
                return;
            }

            console.WriteLine($"Job [{nameWithFolder}] created");
        }
    }

    [Command("copy", Description = "Copy jenkins job")]
    internal class JenkinsJobCopyCommand : JenkinsParameter
    {
        [Option(Description = "Source Job - format: [Folder/Job]", ShortName = "s")]
        [Required]
        public string Source { get; set; }

        [Option(Description = "Target Job - format: [Folder/Job]", ShortName = "t")]
        [Required]
        public string Target { get; set; }

        [Option(Description = "Git Repo Address - http/https or ssh", ShortName = "g")]
        public string GitAddress { get; set; }
        
        public void OnExecute(IConsole console, IJenkinsService jenkinsService)
        {
            var (isValid, host, user, tokenOrPassword) = ValidParameters(console);
            if (!isValid)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(Source))
            {
                console.WriteLine("Source is required");
                return;
            }

            if (string.IsNullOrWhiteSpace(Target))
            {
                console.WriteLine("Target is required");
                return;
            }

            var sourceFolder = string.Empty;
            var sourceJob = Source;

            var targetFolder = string.Empty;
            var targetJob = Target;

            var array = Source.Split('/');
            if (array.Length > 2)
            {
                console.WriteLine("Source format is invalid");
                return;
            }

            if (array.Length == 2)
            {
                sourceFolder = array[0];
                sourceJob = array[1];
            }

            array = Target.Split('/');
            if (array.Length > 2)
            {
                console.WriteLine("Target format is invalid");
                return;
            }

            if (array.Length == 2)
            {
                targetFolder = array[0];
                targetJob = array[1];
            }

            var sourceName = !string.IsNullOrWhiteSpace(sourceFolder) ? $"{sourceFolder}/{sourceJob}" : sourceJob;
            var targetName = !string.IsNullOrWhiteSpace(targetFolder) ? $"{targetFolder}/{targetJob}" : targetJob;

            bool isExisted;

            (var success, var spec) = jenkinsService.GetJobSpec(host, user, tokenOrPassword, sourceFolder, sourceJob);
            if (!success)
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(targetFolder))
            {
                (success, var list) = jenkinsService.ListFolder(host, user, tokenOrPassword);
                if (!success)
                {
                    return;
                }

                isExisted = list.Any(n => (n.Name ?? string.Empty).Equals(targetFolder, StringComparison.OrdinalIgnoreCase));

                if (!isExisted)
                {
                    console.WriteLine($"Target folder [{targetFolder}] is not existed");
                    return;
                }
            }

            (success, isExisted) = jenkinsService.CheckExists(host, user, tokenOrPassword, targetFolder ?? string.Empty, targetJob);
            if (!success)
            {
                return;
            }

            if (isExisted)
            {
                console.WriteLine($"Name [{targetName}] exists");
                return;
            }

            using var client = new RestClient();
            (success, var crumb) = jenkinsService.GetCrumb(client, host, user, tokenOrPassword);
            if (!success || crumb == null)
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(GitAddress))
            {
                spec = Regex.Replace(spec, "<remote>.*</remote>", $"<remote>{GitAddress}</remote>");
            }
            
            success = jenkinsService.CreateJob(client, host, user, tokenOrPassword, targetFolder ?? string.Empty, targetJob, crumb, spec);
            if (!success)
            {
                return;
            }

            console.WriteLine($"Job [{sourceName}] copied to [{targetName}]");
        }
    }
}
