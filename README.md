# MongoDelta

![Azure DevOps builds (branch)](https://img.shields.io/azure-devops/build/adrianbradfield/df1cab1e-21bf-4a8e-a335-29d7a5b730ab/2/master?style=for-the-badge) ![Azure DevOps tests (compact)](https://img.shields.io/azure-devops/tests/adrianbradfield/df1cab1e-21bf-4a8e-a335-29d7a5b730ab/2/master?compact_message&style=for-the-badge) ![Azure DevOps coverage (branch)](https://img.shields.io/azure-devops/coverage/adrianbradfield/df1cab1e-21bf-4a8e-a335-29d7a5b730ab/2/master?style=for-the-badge) ![Nuget](https://img.shields.io/nuget/v/MongoDelta?style=for-the-badge) ![Nuget](https://img.shields.io/nuget/dt/MongoDelta?style=for-the-badge)

Implements a repository pattern with chage tracking for the C# MongoDb driver, and allows you to update only changed properties

This project is currently under initial development and is not ready for use

Aims:

-   Provide change tracking for objects retrieved from MongoDb
-   Implement a repository pattern (Query, Add, Remove)
-   Provide method to commit changes to a repository. This will calculate whether to insert update or delete
-   When an update is needed, provide different update strategies

-   Replace - Whole object to be updated
-   Delta - Only changed properties will be updated

-   Allow different update strategies for each individual property in the mapped object
-   Properties of numeric type (or any property that can be incremented) can also have a delta update strategy will increment the property by the difference rather than completely overwriting the value
-   Properties of IEnumberable type can also have the delta update strategy. This will add, update or remove items from a list rather than overwriting the whole list. This will need a matching strategy for this to work

Technical:

-   Use trackerdog for change tracking
-   Will extend the BSONClassMap class with additional methods for setting update strategies
-   Will need to read from the class map to generate a trackerdog mapping, so we track only the needed properties
