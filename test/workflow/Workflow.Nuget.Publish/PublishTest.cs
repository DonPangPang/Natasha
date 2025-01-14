﻿using NuGet.Versioning;
using Publish.Helper;
using System.Text;

namespace Workflow.Nuget.Publish
{
    [Trait("管道功能", "NUGET")]
    public class PublishTest
    {
        [Fact(DisplayName = "打包检测")]
        public async Task Pass()
        {

            int pushCount = 0;
            bool isWriteEnv = true;

            var message = new StringBuilder();
            message.AppendLine();

            var (version, log) = ChangeLogHelper.GetReleaseInfoFromFromFile(ResourcesHelper.ChangeLogFile);
            if (version!=string.Empty && isWriteEnv)
            {
                var releaseInfo = ChangeLogHelper.GetReleasePackageInfo(log);
                if (releaseInfo == null)
                {
                    message.AppendLine("未获取到发布版本信息!");
                }
                else
                {

                    message.AppendLine($"CHANGELOG.md 扫描信息:");
                    var releasesDict = releaseInfo.ToDictionary(item => item.name, item => item.version);
                    foreach (var item in releasesDict)
                    {
                        message.AppendLine($"\t包名称: {item.Key} ,准备发布版本:{(string.IsNullOrEmpty(item.Value) ? "{空}" : item.Value)};");
                    }
                    message.AppendLine();
                    var projects = CSProjHelper.GetProjectsFromSrc();

                    if (projects != null)
                    {
                        foreach (var (file, project) in projects)
                        {
                            var packageAble = project!.PropertyGroup.IsPackable;
                            var packageName = project!.PropertyGroup.PackageId;
                            if (packageAble == true && packageName != null)
                            {
                                if (releasesDict.ContainsKey(packageName))
                                {
                                    var packageVersion = releasesDict[packageName];
                                    if (!string.IsNullOrEmpty(packageVersion))
                                    {
                                        var latestVersion = await NugetHelper.GetLatestVersionAsync(packageName);
                                        if (latestVersion == null || NuGetVersion.Parse(packageVersion) > latestVersion)
                                        {
                                            //打包并检测该工程能否正常输出 NUGET 包
                                            var result = await NugetHelper.BuildAsync(file) && await NugetHelper.PackAsync(file, packageVersion);
                                            if (result)
                                            {
                                                pushCount += 1;
                                                Assert.True(result);
                                            }
                                        }
                                        else
                                        {
                                            message.AppendLine($"C#工程:包 {packageName} 的准发布版本 {packageVersion} 并不高于 NUGET 仓库中的 {latestVersion} 版本!");
                                        }
                                    }
                                    else
                                    {
                                        message.AppendLine($"CHANGELOG.md: 包 {packageName} 未包含版本信息!");
                                        message.AppendLine($"\t改正如:### {packageName} _ v1.0.0.0:");
                                    }
                                }
                                else
                                {
                                    message.AppendLine($"C#工程:包 {packageName} 未包含在 CHANGELOG.md 中!");
                                    message.AppendLine($"\t改正如:### {packageName} _ vX.X.X.X:");
                                }
                            }
                            else
                            {
                                if (packageName == null)
                                {
                                    message.Append("C#工程:包 {空名} ");
                                }
                                else
                                {
                                    message.Append($"C#工程:包 {packageName} ");
                                }
                                if (packageAble == true)
                                {
                                    message.AppendLine("开启了打包功能.");
                                }
                                else
                                {
                                    message.AppendLine("未开启打包功能(<IsPackable>true</IsPackable>).");
                                }

                            }
                        }
                    }
                    else
                    {
                        message.AppendLine($"未扫描到工程项目中存在正确的发包信息.");
                        message.AppendLine($"请检查 src/ 下的 csproj 文件以及 Directory.Build.props 文件是否正确填写包信息.");
                    }
                }
            }
            else
            {
                message.AppendLine("CHANGELOG.md: 未获取到 Release 发版信息!(## [x.x.x] - 2023-03-08)");
            }

            if (pushCount == 0)
            {
                Assert.Fail(message.ToString());
            }
        }
    }
}