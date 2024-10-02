# Vehicle Rental API

## Introduction
A basic api for vehicle rental service with a reservation availability check.

## Assumptions
For simiplicity I assume that number of vehicles served is not large so that pagination in api response is ignored. 

## Future Enhancements
- Authentication
- Logging
- Unit tests code enhancements like using Theory and Builder pattern

## Challenges
- Reservations conflicts due to concurrency and for that reason I use optimistic concurrency approach with built in concurrency feature of EF.
