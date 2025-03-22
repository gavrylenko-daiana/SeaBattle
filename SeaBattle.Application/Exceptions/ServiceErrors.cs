using SeaBattle.Domain.Models.Errors;

namespace SeaBattle.Application.Exceptions;

public static class ServiceErrors
{
    public static class AppUserServiceExceptions
    {
        public static Error NullOrWhiteSpaceString => new Error(
            "AppUserServiceExceptions.NullOrWhiteSpaceString",
            "The input string is null or white space.");
        
        public static Error NonExistentUser => new Error(
            "AppUserServiceExceptions.NonExistentUser",
            "Such user does not exist.");
        
        public static Error NonExistentUsers => new Error(
            "AppUserServiceExceptions.NonExistentUsers",
            "User list is empty.");
        
        public static Error UserEmailIsAlreadyExists => new Error(
            "AppUserServiceExceptions.UserEmailIsAlreadyExists",
            "A user with such email is already exists.");
        
        public static Error UserNameIsAlreadyExists => new Error(
            "AppUserServiceExceptions.UserNameIsAlreadyExists",
            "A user with such username is already exists.");
        
        public static Error FailedUpdateUser => new Error(
            "AppUserServiceExceptions.FailedUpdateUser",
            "Something went wrong during updating user.");
        
        public static Error InvalidEmail => new Error(
            "AppUserServiceExceptions.InvalidEmail",
            "Invalid email.");
        
        public static Error InvalidPassword => new Error(
            "AppUserServiceExceptions.InvalidPassword",
            "Invalid password.");
        
        public static Error InvalidId => new Error(
            "AppUserServiceExceptions.InvalidId",
            "Invalid identity.");
    }
    
    public static class JwtTokenServiceExceptions
    {
        public static Error ExpiredToken => new Error(
            "JwtTokenServiceExceptions.ExpiredToken",
            "Token has expired.");
        
        public static Error InvalidTokenSignature => new Error(
            "JwtTokenServiceExceptions.InvalidTokenSignature",
            "Invalid token signature.");
        
        public static Error MissingEmail => new Error(
            "JwtTokenServiceExceptions.MissingEmail",
            "Email claim is missing.");
        
        public static Error MissingUserId => new Error(
            "JwtTokenServiceExceptions.MissingUserId",
            "User id claim is missing.");
        
        public static Error ErrorValidatingToken => new Error(
            "JwtTokenServiceExceptions.ErrorValidatingToken",
            "Error validating token.");
        
        public static Error CannotReadToken => new Error(
            "JwtTokenServiceExceptions.CannotReadToken",
            "Cannot read token.");
    }

    public static class ShipCoordinateServiceExceptions
    {
        public static Error FailedCreateShipCoordinate => new Error(
            "ShipCoordinateServiceExceptions.FailedCreateShipCoordinate",
            "Something went wrong during creating new ShipCoordinate.");
    }
    
    public static class ShipServiceExceptions
    {
        public static Error FailedCreateShip => new Error(
            "ShipServiceExceptions.FailedCreateShip",
            "Something went wrong during creating new ship.");
        
        public static Error NonExistentShip => new Error(
            "ShipServiceExceptions.NonExistentShip",
            "Such ship does not exist.");
        
        public static Error CannotConvertDirection => new Error(
            "ShipServiceExceptions.CannotConvertDirection",
            "Cannot convert direction string into direction type.");
    }
    
    public static class CoordinateServiceExceptions
    {
        public static Error FailedCreateCoordinate => new Error(
            "CoordinateServiceExceptions.FailedCreateCoordinate",
            "Something went wrong during creating new coordinate.");
        
        public static Error NonExistentCoordinate => new Error(
            "CoordinateServiceExceptions.NonExistentCoordinate",
            "Such coordinate does not exist.");
    }
    
    public static class PointServiceExceptions
    {
        public static Error FailedCreatePoint => new Error(
            "PointServiceExceptions.FailedCreatePoint",
            "Something went wrong during creating new point.");
    }
    
    public static class GameFieldServiceExceptions
    {
        public static Error FailedCreateGameField => new Error(
            "GameFieldServiceExceptions.FailedCreateGameField",
            "Something went wrong during creating new game field.");
        
        public static Error NonExistentGameField => new Error(
            "GameFieldServiceExceptions.NonExistentGameField",
            "Such game field does not exist.");
        
        public static Error NoValidPlacement => new Error(
            "GameFieldServiceExceptions.NoValidPlacement",
            "There is no suitable place for the ship.");
    }
    
    public static class UserGameFieldServiceExceptions
    {
        public static Error NonExistentUserGame => new Error(
            "UserGameFieldServiceExceptions.NonExistentUserGameField",
            "Such user game does not exist.");
    }

    public static class GameServiceExceptions
    {
        public static Error UserHasAlreadyJoinedThisGame => new Error(
            "GameServiceExceptions.UserHasAlreadyJoinedThisGame",
            "The user has already joined this game.");
        
        public static Error GameHasAlreadyStarted => new Error(
            "GameServiceExceptions.GameHasAlreadyStarted",
            "The game has already started.");
        
        public static Error FailedCreateGame => new Error(
            "GameServiceExceptions.FailedCreateGame",
            "Something went wrong during creating new game.");
        
        public static Error NonExistentGame => new Error(
            "GameServiceExceptions.NonExistentGame",
            "Such game does not exist."); 
        
        public static Error NonExistentGames => new Error(
            "GameServiceExceptions.NonExistentGames",
            "Game list is empty.");
        
        public static Error UnableToRetrieveUserRating => new Error(
            "GameServiceExceptions.UnableToRetrieveUserRating",
            "Unable to retrieve user rating.");
        
        public static Error NoSuitableGame => new Error(
            "GameServiceExceptions.NoSuitableGame",
            "No suitable game found within the time limit.");
        
        public static Error NoSuitableOpponent => new Error(
            "GameServiceExceptions.NoSuitableOpponent",
            "No suitable opponent found within the time limit.");
    }

    public static class GameInvitationServiceExceptions
    {
        public static Error InvitationNotFound => new Error(
            "GameInvitationServiceExceptions.InvitationNotFound",
            "Invitation was not found.");
        
        public static Error InvitationIsNotAvailable => new Error(
            "GameInvitationServiceExceptions.InvitationIsNotAvailable",
            "The invitation is not available or has already been accepted.");
        
        public static Error InvitationAlreadyExists => new Error(
            "GameInvitationServiceExceptions.InvitationAlreadyExist",
            "Such invitation is already exists.");
    }

    public static class CoordinateTypeServiceExceptions
    {
        public static Error CoordinateTypeNotFound => new Error(
            "CoordinateTypeServiceExceptions.CoordinateTypeNotFound",
            "Coordinate type was not found.");
    }

    public static class UnitOfWorkExceptions
    {
        public static Error ImpossibleCommitChanges => new Error(
            "UnitOfWorkExceptions.ImpossibleCommitChanges",
            "The changes was not commit.");
    }
}