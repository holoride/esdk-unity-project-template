### System Requirements

- **Operating System for Unity Editor:** Windows 10, Windows 11, macOS (Experimental)
- **Minimum supported Unity version:** 2020.3.15 (LTS)
- **Stable internet connection**

This readme is an excerpt of the official Elastic SDK Documentation as found at: https://elasticsdk.holoride.com

### Get the Elastic SDK

To download the Elastic SDK, you have to use Unity’s built-in package manager (UPM). To do this via UPM, we are using
UPM registry authentication (introduced with Unity 2020.3).

First you need to download a custom Unity Package Manager configuration file `.upmconfig.toml` to get access to our
server:
Please get it [from the "Set me up" page](https://developer.holoride.com/set_me_up).

- On Windows, place the downloaded `.upmconfig.toml` file downloaded into your `%USERPROFILE%` folder.

- On MacOS, place the downloaded `.upmconfig.toml` should be placed at the home directory `~/`.

Double-check if the file name contains a leading dot because it may have been removed by your browser. Otherwise rename
the file into `.upmconfig.toml`. If the file already exists, append the contents of the downloaded file to the already
existing one using a text editor of your choice. For example, open both in a text editor and copy the lines from downloaded file into the existing file.

If you are creating the `.upmconfig.toml` file yourself, be sure to edit the file extension and remove
any extension that comes after `.toml`. You can confirm this by right clicking on the `.upmconfig.toml` file and
clicking **Properties**. Here, under **General** tab, **Type of File** should correspond to `.TOML File (.toml)`. If
not (e.g. it is a .txt file), click *Alt-V* to open the File Explorer View Menu. In that menu, click on the checkbox
for **File Name Extensions**. Now you can remove the ".txt" extension from your upmconfig.toml file.

### Enable your Unity project

Clone this repository and open the root folder in the Unity Hub with the corresponding Unity version.
It's very likely that slightly different Unity versions will also work without any issues.

You need to link a manifest file specifically created for
your project. It can be created and downloaded from
the [projects page in the Creator Space](https://developer.holoride.com/projects).

#### Project Settings

To use the Elastic SDK you have to link a project specific **holoride manifest file** and enter your **personal Creator
Space credentials** under **Project Settings > holoride**

Under **Project Settings > Player Settings**, change the company name and the project name according to your creator space setup.

If you received an production manifest from holoride, also override the package name in the **Identification** category. 

#### Manifest File

You can generate a manifest file for each content piece / project [here](https://developer.holoride.com/projects) in the
Creator Space. If you decide to create multiple projects, you’ll have to create and download a new manifest file for
each project from the Creator Space. The manifest file is bound to your project by its name. Make sure the project name
configured in **Project Settings >
Player** in the Unity Editor matches the name of the project in the Creator Space.

#### Holoride Credentials

To use the Elastic SDK in Unity, you have to log in with your holoride Creator Space credentials.

### Validate Your Project

After successfully importing the ElasticSDK package with all its dependencies, see the holoride Project Validation
window to set up your project and fix all points on the list that show an exclamation mark (Check the Inspector when
clicking **Select**). You can also find it the main menu under **holoride > Project Validation**, if you accidentally
closed it.