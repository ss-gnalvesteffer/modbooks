# Writeable Books | Durable Quills and Pencils
**Updated by Xorberax for Vintage Story 1.15.1**

Adds writeable books to Vintage Story. This feature branch makes it so that quills and pencils have durability, and degrade during writing.

See the wiki for more info about this mod: https://github.com/cloutech/modbooks/wiki


## Building the project and packaging it for distribution
- Open the solution file `src/Books/Books.sln`.
- Change the file paths of the `VintagestoryAPI` and `VSEssentials` dll references for the `Books` project to point to the ones in your environment.
- Build the project.
- The mod content will be built and packaged as `Books.zip` in the output parent directory.