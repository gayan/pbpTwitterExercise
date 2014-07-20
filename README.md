pbpTwitterExercise
==================

Pay By Phone Twitter Exercise. Step 2 of the interview process.

NOTES FOR REVIEWER:
* I only needed access to some endpoints for this exercise, and it sufficed to use a "bearer" token to access them on behalf of the test app that I built. As such it was trivial to implement, so I did not use a 3rd party OAuth library.

* I would love to have an opportunity to explain my work, as there are assumptions, and choices I made a long the way, that might not by typical for everyone.

* I have used unity.mvc4 for dependency injection, to ease unit testing.

* My unit testing code is generally less well organized than the application code I write. You may notice that :)

* I have tried to leave ample comments to explain what I am doing. 

* I attempted to optimize the merging of k twitter feeds into one list by using a sorted list, and a heap as the data structure. However, it did not seem any better than the Sorting algorithm that is included in the .net framework. I left some of that work commented out, as an example of my attention to time complexity.