using McMaster.Extensions.CommandLineUtils;
using Newtonsoft.Json;
using RestSharp;
using System.ComponentModel.DataAnnotations;

namespace PowerCI.Commands.Jenkins
{
    [Command("job", Description = "Handle jenkins jobs"), 
     Subcommand(typeof(JenkinsJobListCommand)),
     Subcommand(typeof(JenkinsJobCreateCommand))]
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
        public string? Folder { get; set; }

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
        public string? Folder { get; set; }

        [Option(Description = "Job Name", ShortName = "n")]
        [Required]
        public string? Name { get; set; }

        [Option(Description = "Job Type - freestyle, pipeline, multibranch", ShortName = "t")]
        public string? Type { get; set; }

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
}
