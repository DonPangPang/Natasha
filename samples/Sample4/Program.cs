﻿using Microsoft.Extensions.DependencyModel;
using Natasha.CSharp;
using Natasha.CSharp.Builder;
using System;

namespace Sample4
{
    internal class Program
    {
        static void Main(string[] args)
        {

            var defaul1 = DependencyContext.Default;
            NatashaManagement.Preheating();
            NatashaManagement.AddGlobalUsing("System");
            //NatashaManagement.AddGlobalUsing("NatashaDynimacSpace");
            NatashaReferenceCache.AddReference(typeof(object).Assembly);
            NatashaReferenceCache.AddReference(typeof(Console).Assembly);
            //var domain = NatashaManagement.CreateRandomDomain();
            //using (domain.CreateScope())
            //{
            //    var action1 = NDelegate
            //        .UseScope()
            //        .Action("Console.WriteLine(\"Hello World!\");");

            //    action1();
            //}




            var action1 = NDelegate
                    //使用随机域 也可以使用 CreateDomain / UseDomain / DefaultDomain 
                    .DefaultDomain()
                    //[可选API] 必要时使用 ConfigBuilder 配置编译单元(下面只为展示API, 无实际意义)
                    .ConfigBuilder(builder => builder
                        //配置编译器选项
                        .ConfigCompilerOption(opt => opt
                            //配置平台
                            .SetPlatform(Microsoft.CodeAnalysis.Platform.AnyCpu)
                            //Release 方式编译
                            .CompileAsRelease()
                            //开启可空警告
                            .SetNullableCompile(Microsoft.CodeAnalysis.NullableContextOptions.Warnings))
                        //配置语法选项
                        .ConfigSyntaxOptions(opt => opt
                            //配置支持的脚本语言版本
                            .WithLanguageVersion(Microsoft.CodeAnalysis.CSharp.LanguageVersion.CSharp8))
                        //禁用语义检查
                        .DisableSemanticCheck()
                        )
                    //[可选API] 配置该方法所在的类模板
                    .ConfigClass(item => item
                        //给类配置一个名字,不用随即名
                        .Name("myClass")
                        //不使用默认域的 Using 缓存
                        .NoGlobalUsing()
                        )
                    .ConfigUsing("System")
                    //这里的API 参照定义的委托, 包括委托的参数
                    //例如 Action<int> / Func<int,int> 拥有一个参数, 参数的名字请在 Action<int> / Func<int,int> 上F12 查看定义.
                    .Action("Console.WriteLine(\"Hello World!\");");

            action1();


            //简写
            var action2 = NDelegate
                    .DefaultDomain()
                    .Action("Console.WriteLine(\"Hello World!\");");

            action2();


            NClass nClass = NClass.DefaultDomain();
            nClass
              .Namespace("MyNamespace")
              .Public()
              .Name("MyClass")
              .Ctor(ctor => ctor.Public().Body("MyField=\"Hello\";"))
              .Property(prop => prop
                .Type(typeof(string))
                .Name("MyProperty")
                .Public()
                .OnlyGetter("return \"World!\";")
              );


            //添加方法
            MethodBuilder mb = new MethodBuilder();
            mb
              .Public()
              .Override()
              .Name("ToString")
              .Body("return MyField+\" \"+MyProperty;")
              .Return(typeof(string));
            nClass.Method(mb);


            //添加字段
            nClass.Field(fb => fb.Public()
              .Name("MyField")
              .Type<string>());

            var type = nClass.GetType();

            //动态调用动态创建的类
            var action3 = NDelegate
              .DefaultDomain()
              .Action("Console.WriteLine((new MyClass()).ToString());");

            action3();

            Console.ReadKey();
        }
    }
}