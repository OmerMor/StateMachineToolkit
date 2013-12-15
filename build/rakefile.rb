require 'rake/clean'

#task :default do
#    $stderr.puts "Howdy, World"


DOT_NET_PATH = "#{ENV["SystemRoot"]}\\Microsoft.NET\\Framework\\v4.0.30319"
NUNIT_EXE = "../packages/NUnit.Runners.2.6.3/tools/nunit-console.exe"
NUGET_EXE = "../packages/NuGet.CommandLine.2.7.3/tools/NuGet.exe"
ROOT_PATH = ".."
SOURCE_PATH = "../src"
OUTPUT_PATH = "../output"
configurations = ["Debug_NET20","Debug_NET40","Release_NET20","Release_NET40"]
 
CLEAN.include(OUTPUT_PATH)

task :default => ["clean", "build:all"]
 
namespace :build do
  
    task :all => [:compile, :test, :pack]
      
    desc "Build solutions using MSBuild"
    task :compile do
        solutions = FileList["#{ROOT_PATH}/**/*.sln"]
        solutions.each do |solution|
            configurations.each do |config|
                sh "#{DOT_NET_PATH}/msbuild.exe /p:Configuration=#{config} #{solution}"
            end
        end
    end
   
    desc "Runs tests with NUnit"
    task :test => [:compile] do
        tests = FileList["#{OUTPUT_PATH}/**/*.Tests.dll"].exclude(/obj\//)
        sh "#{NUNIT_EXE} #{tests} /nologo /xml=#{OUTPUT_PATH}/TestResults.xml"
    end

    desc "Package with NuGet"
    task :pack => [:compile] do
        specs = FileList["#{SOURCE_PATH}/**/*.nuspec"]
        specs.each do |spec|
            sh "#{NUGET_EXE} pack #{spec} -OutputDirectory #{OUTPUT_PATH}"
        end
    end	
    
    desc "Publish with NuGet"
    task :publish => [:pack] do
        packages = FileList["#{OUTPUT_PATH}/*.nupkg"]
        packages.each do |package|
            sh "#{NUGET_EXE} push #{package}"
        end
    end	
end
