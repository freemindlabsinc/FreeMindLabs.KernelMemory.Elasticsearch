## Diary

>A bunch of notes and thoughts about the project.

:calendar: 12/05/2023

1. Working on SK Hackathon: Code Mapper project.

:calendar: 12/04/2023
1. Version 0.3.0
1. Implemented most of the methods of IMemoryDb
    1. Need to finish MemoryFilter implementation
1. General repo cleanup

:calendar: 12/01/2023
1. Version 0.2.0
1. Added this DIARY .md file to the project.  
1. Merged with the new KM nuget 0.15.231130.2-preview
1. Cleaned up the repo a lot after merging with the [kernel-memory-postgres repository](https://github.com/microsoft/kernel-memory-postgres).
    1. Pages like LICENSE, README, etc. have been 'ported' from the same repository.
    1. The analyzers are awesome. We essentially standardized to MS' conventions.
       1. Changed editor .editorconfig to be for FML
1. Improved the configuration setup in UnitTests/Startup.cs
    1. Determined how to better structure configuration options.
    1. Created several extensions, including one to go from ElasticsearchConfig to ElasticsearchClientSettings
      1. Removed code from the TestApplication into the UnitTest project 
        1. This is a better place for it.        
