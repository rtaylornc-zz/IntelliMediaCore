# IntelliMediaCore
Collection of C# design patterns (MVVM, Repository, Promise, Singleton) and utility classes. The class library is intended to be used in conjunction with Unity 3D game engine projects or Mono-based servers and tools.

**NOTE:** This is an **alpha** release of the class library. There are no binary distributions available at this time. The preferred approach is to create a Git subtree of the IntelliMediaCore/Source in your project's Git repository.  

## Build requirements
Either of these environments can be used to build IntelliMediaCore:
* [Unity 5 Game Engine](https://unity3d.com/)
* [Xamarin Studio 5](https://xamarin.com)

## How-to integrate into a Unity 5 project

Create a Git subtree within your project's repository to facilitate pushing fixes and new code back into IntelliMediaCore or simply take a snapshot of the source and place the IntelliMediaCore/Source dirctory in your Unity project's **Assets** directory.

1. Change directory into your Unity 5 project's *project* directory (the root directory that contains Assets, ProjectSettings, and other directories).
1. Add a remote URL pointing to IntelliMediaCore
 * `$ git remote add -f IntelliMediaCore git@github.com:IntelliMedia/IntelliMediaCore.git`
1. Merge into your local Git repository
 * `$ git merge -s ours --no-commit IntelliMediaCore/master`
1. Update your Unity project with the contents of IntelliMediaCore's master branch in a new directory *Assets/IntelliMediaCore*. Run this command inside your Unity 5 project directory.
 * `$ git read-tree --prefix=Assets/IntelliMediaCore/ -u IntelliMediaCore/master`  
1. Finalize the changes with a commit.
 * `$ git commit -m "IntelliMediaCore added as a subtree in Assets directory"`  

## Contributing

We welcome contributions of new features, improvements, and bug fixes from the community. However, since this is our first attempt at an open source project, we haven't quite figured out the logisitics, yet. Contact us at intellimedia@ncsu.edu if you are interested in contributing. Once we have a policy we'll update this text.

## Copyright

Copyright 2014 North Carolina State University

Licensed under the [Simplified BSD License](LICENSE.md)