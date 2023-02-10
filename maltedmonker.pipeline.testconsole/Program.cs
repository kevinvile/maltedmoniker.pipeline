using maltedmoniker.pipeline;
using maltedmoniker.pipeline.Builders;
using maltedmoniker.pipeline.Factories;
using maltedmoniker.pipeline.Pipelines;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;


namespace maltedmonker.pipeline.testconsole
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var pipelineBuilderFactory = new PipelineBuilderFactory();

            Console.WriteLine("Hello World!");
            {
                var files = GetMyFiles();
                using var myFilePipeline = GetMyFilePipeline(pipelineBuilderFactory);
                myFilePipeline.DoNotUsePipelineContext();

                await foreach (var processed in myFilePipeline.Process(files))
                {
                    Console.WriteLine(processed);
                }

                Console.WriteLine("\r\n\r\nOriginals\r\n");

                foreach (var file in files)
                {
                    Console.WriteLine(file);
                }
            }

            await RunPipeline(true);
            await RunPipeline(false);

            await RunParallelPipeline(pipelineBuilderFactory, 200, true);
            await RunParallelPipeline(pipelineBuilderFactory, 200, false);
   
            await RunChannelPipeline(pipelineBuilderFactory, 300, true);
            await RunChannelPipeline(pipelineBuilderFactory, 300, false);
        }

        static async Task RunPipeline(bool noPipelineContext)
        {
            Console.WriteLine($"\r\n--- Single Threaded Pipeline (No Context: {noPipelineContext}) ---");

            var builder = new PipelineBuilder<Pipeline<A, C>, A, C>();
            using var pipeline = builder
                .WithStep(new AToASync())
                .WithStep(new AToBSync())
                .WithStep(new BToCSync())
                .WithStep(new CToCSync())
                .Build();

            if(noPipelineContext) pipeline.DoNotUsePipelineContext();

            //Warm ups
            Console.WriteLine("\r\n--- Warming up ---");
            await RunATest(pipeline, 10, true);
            await RunATest(pipeline, 100, true);
            await RunATest(pipeline, 300, true);
            await RunATest(pipeline, 800, true);

            await RunATest(pipeline, 10);
            await RunATest(pipeline, 100);
            await RunATest(pipeline, 300);
            await RunATest(pipeline, 800);
        }

        static async Task RunParallelPipeline(PipelineBuilderFactory pipelineBuilderFactory, int parallelSize, bool noPipelineContext)
        {
            Console.WriteLine($"\r\n--- Parallel Pipeline ({parallelSize}, No Context: {noPipelineContext}) ---");

            var builder2 = pipelineBuilderFactory.GetParallelBuilder<A, C>();
            using var pipeline2 = builder2
                .WithStep(new AToASync())
                .WithStep(new AToBSync())
                .WithStep(new BToCSync())
                .WithStep(new CToCSync())
                .Build();

            pipeline2.ParallelSize = parallelSize;
            if(noPipelineContext) pipeline2.DoNotUsePipelineContext();

            //Warm ups
            Console.WriteLine("\r\n--- Warming up ---");
            await RunATest(pipeline2, 10, true);
            await RunATest(pipeline2, 100, true);
            await RunATest(pipeline2, 1000, true);
            await RunATest(pipeline2, 10000, true);
            await RunATest(pipeline2, 100000, true);
            await RunATest(pipeline2, 1000000, true);


            await RunATest(pipeline2, 10);
            await RunATest(pipeline2, 100);
            await RunATest(pipeline2, 1000);
            await RunATest(pipeline2, 10000);
            await RunATest(pipeline2, 100000);
            await RunATest(pipeline2, 1000000);
        }

        static async Task RunChannelPipeline(PipelineBuilderFactory pipelineBuilderFactory, int channelSize, bool noPipelineContext)
        {
            Console.WriteLine($"\r\n--- Channel Pipeline ({channelSize}, No Context: {noPipelineContext}) ---");

            var builder3 = pipelineBuilderFactory.GetChannelBuilder<A, C>();
            using var pipeline3 = builder3
                .WithStep(new AToASync())
                .WithStep(new AToBSync())
                .WithStep(new BToCSync())
                .WithStep(new CToCSync())
                .Build();

            pipeline3.ChannelSize = channelSize;
            if(noPipelineContext) pipeline3.DoNotUsePipelineContext();

            //Warm ups
            Console.WriteLine("\r\n--- Warming up ---");
            await RunATest(pipeline3, 10, true);
            await RunATest(pipeline3, 100, true);
            await RunATest(pipeline3, 1000, true);
            await RunATest(pipeline3, 10000, true);
            await RunATest(pipeline3, 100000, true);
            await RunATest(pipeline3, 1000000, true);

            await RunATest(pipeline3, 10);
            await RunATest(pipeline3, 100);
            await RunATest(pipeline3, 1000);
            await RunATest(pipeline3, 10000);
            await RunATest(pipeline3, 100000);
            await RunATest(pipeline3, 1000000);
        }

        static async Task RunATest(Pipeline<A, C> pipeline, int theAsToRun, bool noReporting = false)
        {
            List<A> theAs = new();
            for (int i = 0; i < theAsToRun; i += 1)
            {
                theAs.Add(new A($"A{i}"));
            }
            int ran = 0;

            //Console.WriteLine("Running A records");
            var sw = Stopwatch.StartNew();
            sw.Start();

            await foreach (var processed in pipeline.Process(theAs))
            {
                //Console.Write($"{processed.Name} ");
                ran += 1;
            }

            sw.Stop();
            if (noReporting) return;

            Console.WriteLine();
            Console.WriteLine($"Ran {ran} A records in {sw.ElapsedMilliseconds}ms:  {(float)theAsToRun / sw.ElapsedMilliseconds} records per ms, {sw.ElapsedMilliseconds / (float)theAsToRun} ms per record");
        }

        static MyFile[] GetMyFiles()
        {
            MyFile[] files = new MyFile[2];
            files[0] = new MyFile { FileName = "Test1" };
            files[1] = new MyFile { FileName = "Test2" };
            return files;
        }

        static Pipeline<MyFile, MyFile2> GetMyFilePipeline(IPipelineBuilderFactory pipelineBuilderFactory)
        {
            return pipelineBuilderFactory.GetBuilder<MyFile, MyFile2, Pipeline<MyFile, MyFile2>>()
                    .WithStep(new BackupMyFile())
                    .WithStep(new Something())
                    .Build();
        }
    }

    record MyFile : Immutable
    {
        public string FileName { get; init; } = string.Empty;
        public bool IsBackedUp { get; private set; }
        public bool IsSomethingElse { get; private set; }

        public void MarkBackedUp()
        {
            IsBackedUp = true;
        }

        public void MarkSomethingElse()
        {
            IsSomethingElse = true;
        }

        public override string ToString()
        {
            return $"{FileName} is something else: {IsSomethingElse}, is backed up: {IsBackedUp}";
        }

    }

    record MyFile2  
    {

    }

    class BackupMyFile : ISyncPipe<MyFile, MyFile>
    {
        public MyFile Execute(MyFile item, PipelineContext context)
        {
            Console.WriteLine($"Backing up {item.FileName}");
            item.MarkBackedUp();
            item = new MyFile { FileName = "Hacked!" };
            return item;
        }
    }

    class Something : IAsyncPipe<MyFile, MyFile2>
    {
        public async Task<MyFile2> ExecuteAsync(MyFile item, PipelineContext context,  CancellationToken token = default)
        {
            Console.WriteLine($"Doing something {item.FileName}");
            item.MarkSomethingElse();
            await Task.Delay(25, token);
            return new MyFile2();
        }
    }

    public record A(string Name); //: Immutable;
    record B(string Name);
    record C(string Name);
    /*
    class A
    {
        public string Name { get; init; }
        public A(string name)
        {
            Name = name;
        }
    }
    class B
    {
        public string Name { get; init; }
        public B(string name)
        {
            Name = name;
        }
    }
    class C
    {
        public string Name { get; init; }
        public C(string name)
        {
            Name = name;
        }
    }
    */

    class AToASync : ISyncPipe<A, A>
    {
        public A Execute(A item, PipelineContext context)
        {
            return new A(item.Name + " A");
        }
    }

    class AToBAsync : IAsyncPipe<A, B>
    {
        public async Task<B> ExecuteAsync(A item, PipelineContext context, CancellationToken token = default)
        {
            await Task.Delay(1, token);
            return new B(item.Name + " B");
        }
    }

    class AToBSync : ISyncPipe<A, B>
    {
        public B Execute(A item, PipelineContext context)
        {
            return new B(item.Name + " B");
        }
    }
    class BToCAsync : IAsyncPipe<B, C>
    {
        public Task<C> ExecuteAsync(B item, PipelineContext context, CancellationToken token = default)
        {
            //            await Task.Delay(10);
            return Task.FromResult(new C(item.Name + " C"));
        }
    }

    class BToCSync : ISyncPipe<B, C>
    {
        public C Execute(B item, PipelineContext context)
        {
            return new C(item.Name + " C");
        }
    }

    class CToCSync : ISyncPipe<C, C>
    {
        public C Execute(C item, PipelineContext context)
        {
            return new C(item.Name + " C");
        }
    }
}
