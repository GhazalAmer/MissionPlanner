# MAP Planner

## (!) This will be the only LOCAL PRIVATE version of the repo and the public one on github will be deleted.
## (!) DON"T EVER DO COMMITS ON THE MASTER BRANCH (easier to keep up to date)

## (*) Update master branch with original repo (keep it up to date with official MP):

    1. git remote add upstream https://github.com/ArduPilot/MissionPlanner.git
    2. git pull upstream master
        (!) If you have altered the master branch, you then need to rebase it.

                git rebase upstream/master

            and resolve the conflict if any.
    3. git checkout MAP-GUI
    4. git merge master
        (*) resolve conflicts, and keep in mind you need to accept the current stream changes for the Layout related code.
        
## (*) MAP-GUI branch is the main Marakeb Version branch.
## (*) Each major feature shoud be in a seperate branch (branch it from MAP-GUI)
## (*) Once you finished coding the new feature, merge its branch into MAP-GUI
        BUT, you should merge MAP-GUI into you feature branch first and resolve conflicts.
 