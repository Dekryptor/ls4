package de.jandev.ls4apiserver.utility;

public class LogMessage {

    public static final String UNHANDLED_EXCEPTION = "An unhandled exception occurred.";
    public static final String UNHANDLED_EXCEPTION_OUT = "An unhandled exception occurred. Request most likely not processed.";
    public static final String REQUEST_NOT_ALLOWED = "Method not allowed.";
    public static final String REQUEST_NOT_READABLE = "Request not readable. Please verify the data.";
    public static final String REQUEST_FORBIDDEN = "User is not authorized to request this resource.";
    public static final String JWT_AUTHENTICATION_ERROR = "JWT authentication encountered an error.";
    public static final String JWT_INVALID_OR_EXPIRED = "JWT Token '{}' invalid or expired.";
    public static final String USER_AUTH_INVALID = "Authentication check failed.";
    public static final String USER_NOT_FOUND = "Could not find user '{}'.";
    public static final String USER_NOT_FOUND_TOKEN = "Could not find user that belongs to the email token '{}'.";
    public static final String USER_NOT_FOUND_EMAIL = "Could not find user with email address '{}'.";
    public static final String USER_REQUEST_CONFIRM = "User '{}' requested an email confirmation to '{}'. Token generated: '{}'.";
    public static final String USER_CONFIRM = "User '{}' confirmed their email address '{}' with token '{}'.";
    public static final String USER_ALREADY_CONFIRMED = "User has already confirmed their email address.";
    public static final String USER_MAILING_DISABLED = "Mailing is not enabled. The use of this endpoint is therefor forbidden.";
    public static final String USER_NO_PERMISSION = "User '{}' is not authorized to request this resource.";
    public static final String USER_ALREADY_EXISTS = "User with email '{}' or summoner name '{}' or user name '{}' already exists.";
    public static final String USER_CREATED = "User '{}' with Summoner Name '{}', User Name '{}' and Email '{}' created.";
    public static final String USER_CHANGE_PASSWORD = "User '{}' ('{}') changed their password.";
    public static final String USER_FRIEND_ADD = "User '{}' added User '{}' as a friend.";
    public static final String USER_FRIEND_ADD_BLOCKED = "User '{}' has been blocked by User '{}', so therefore a friend request cannot be sent.";
    public static final String USER_FRIEND_REMOVE = "User '{}' removed User '{}' as a friend.";
    public static final String USER_FRIEND_DECLINE = "User '{}' declined friend request from User '{}'.";
    public static final String USER_FRIEND_BLOCK = "User '{}' blocked User '{}'.";
    public static final String USER_LOGGED_IN = "User '{}' logged in and got token '{}'.";
    public static final String USER_LOGGED_IN_FAILED = "User '{}' tried to login with a wrong password/username.";
    public static final String USER_LOGGED_IN_FAILED_EMAIL = "User '{}' tried to login without confirming their email first.";
    public static final String USER_MOTTO_TOO_LONG = "Motto exceeds maximum length of 30 characters.";
    public static final String USER_STATUS_INVALID = "Status '{}' is not a valid user status.";
    public static final String USER_LOGGED_IN_ANOTHER_LOCATION = "User logged in from another location.";
    public static final String USER_BANNED = "User cannot login because they're banned.";
    public static final String FRIEND_CONFLICT = "User '{}' is already in the friend list, or a request has already been sent.";
    public static final String FRIEND_SAME_USER = "User '{}' cannot add itself as friend.";
    public static final String FRIEND_NOT_FOUND = "Could not find user '{}' as a friend.";
    public static final String FRIEND_LIMIT_EXCEEDED = "Could not add user '{}' because you reached the maximum amount of friends.";
    public static final String FRIEND_LIMIT_EXCEEDED_TARGET = "Could not add user '{}' because the users friend list is full.";
    public static final String FRIEND_REQUEST_LIMIT_EXCEEDED = "Could not add user '{}' because you have too many outgoing friend requests.";
    public static final String FRIEND_REQUEST_LIMIT_EXCEEDED_TARGET = "Could not add user '{}' because the user has too many pending incoming friend requests.";
    public static final String ICON_NOT_FOUND = "Could not find icon '{}'.";
    public static final String ICON_INVALID = "Icon '{}' is not a valid Integer.";
    public static final String ICON_NOT_OWNED = "User does not own icon '{}'.";
    public static final String ICON_UPDATED = "Icon from user '{}' set to '{}'.";
    public static final String MOTTO_UPDATED = "Motto from user '{}' set to '{}.";
    public static final String STATUS_UPDATED = "Status from user '{}' set to '{}.";
    public static final String CHAMPION_NOT_FOUND = "Could not find champion '{}'.";
    public static final String WEBSOCKET_CONNECTED_QUEUE = "User '{}' connected to websocket queue '{}'.";
    public static final String WEBSOCKET_DISCONNECTED_QUEUE = "User '{}' disconnected from websocket queue '{}'.";
    public static final String WEBSOCKET_DISCONNECTED = "User '{}' disconnected from websocket system.";
    public static final String WEBSOCKET_DISCONNECTED_FORCE = "User '{}' forcefully disconnected from websocket system.";
    public static final String WEBSOCKET_MESSAGE_RECEIVED = "Received websocket message '{}' request with id '{}' from '{}' with data: '{}' on queue '{}'.";
    public static final String CHAT_PRIVATE_CREATED = "User '{}' sent a message to '{}'. Message was: '{}'.";
    public static final String LOBBY_CREATED = "User '{}' created the lobby with id '{}'.";
    public static final String LOBBY_INVITED = "User '{}' invited user '{}' to the lobby with id '{}'.";
    public static final String LOBBY_INVITE_ACCEPTED = "User '{}' accepted the invite to the lobby with id '{}'.";
    public static final String LOBBY_INVITE_DENIED = "User '{}' denied the invite to the lobby with id '{}'.";
    public static final String LOBBY_INVITE_INVALID = "User '{}' has no valid invitation to the lobby with id '{}'.";
    public static final String LOBBY_INVITE_LIMIT_REACHED = "User '{}' could not be invited because he has too many invites.";
    public static final String LOBBY_FULL = "User '{}' cannot accept the invitation to the lobby with id '{}' because it is full.";
    public static final String LOBBY_ALREADY_MEMBER = "User '{}' is already a member of the lobby with id '{}'.";
    public static final String LOBBY_ALREADY_INVITED = "User '{}' was already invited, or is already a member to/of the lobby with id '{}'.";
    public static final String LOBBY_NOT_FOUND = "Could not find lobby '{}'.";
    public static final String LOBBY_DELETED = "Lobby '{}' was disbanded.";
    public static final String LOBBY_NO_MEMBER = "User '{}' is not in the lobby with id '{}'.";
    public static final String LOBBY_LEFT = "User '{}' left the lobby with id '{}'.";
    public static final String LOBBY_KICKED = "User '{}' kicked user '{}' from the lobby with id '{}'.";
    public static final String LOBBY_NO_OWNER = "User '{}' is not owner of the lobby with id '{}' and therefore cannot do this action.";
    public static final String LOBBY_TYPE_UPDATED = "User '{}' updated the lobby type of lobby with id '{}' to '{}'.";
    public static final String LOBBY_TYPE_INVALID = "Type '{}' is not a valid lobby type.";
    public static final String LOBBY_UNAUTHORIZED = "User '{}' is not a member of lobby '{}' and therefore not allowed to subscribe it.";
    public static final String LOBBY_MATCHMAKING_ACCEPT_IMPOSSIBLE = "Lobby is not in queue / did not found a match so therefore this request is invalid.";
    public static final String LOBBY_SWITCH_IMPOSSIBLE = "Lobby is not in custom game mode. Switch is not possible and this request is therefore invalid.";
    public static final String LOBBY_IN_QUEUE_CANNOT_BE_JOINED = "Lobby is currently in queue and cannot be joined.";
    public static final String LOBBY_NOT_ACCEPTED_DEBUG = "Lobby '{}' did not accept with member size of '{}' and accepted size of '{}'";
    public static final String LOBBY_REPUSH_DEBUG = "Lobby '{}' did accept but someone else didn't. They were repushed into the queue.'";
    public static final String MATCHMAKING_ALREADY_IN_QUEUE = "Lobby '{}' is already in queue.";
    public static final String MATCHMAKING_NOT_IN_QUEUE = "Lobby '{}' is not in queue.";
    public static final String MATCHMAKING_BLOCKED_JOIN = "Lobby '{}' is blocked from joining the queue. (Should only happen during ACCEPT phase.)";
    public static final String MATCHMAKING_BLOCKED_JOIN_DODGE = "Lobby '{}' is blocked from joining the queue, because the players '{}' still have a dodge timer.";
    public static final String MATCHMAKING_BLOCKED_LEAVE = "Lobby '{}' is blocked from leaving the queue. (Should only happen during ACCEPT phase.)";
    public static final String MATCHMAKING_BLOCKED_LEAVE_DEBUG = "Lobby '{}' was blocked from leaving the queue. (INTERNAL DEBUG.)";
    public static final String MATCHMAKING_BLOCKED_JOIN_DEBUG = "Lobby '{}' was blocked from re-joining the queue. (INTERNAL DEBUG.)";
    public static final String MATCHMAKING_JOINED_QUEUE = "Lobby '{}' joined the queue for '{}'.";
    public static final String MATCHMAKING_REJOINED_QUEUE = "Lobby '{}' rejoined the queue for '{}'.";
    public static final String MATCHMAKING_LEFT_QUEUE = "Lobby '{}' left the queue for '{}'.";
    public static final String MATCHMAKING_MATCHED_LOBBY = "Found pre game lobby for '{}'.\nTeam 1 lobbies: '{}'\nTeam 2 lobbies: '{}'.";
    public static final String CHAMPSELECT_ADDED = "Added game lobby with id '{}' to active champ select lobbies. Init phase: '{}'.";
    public static final String CHAMPSELECT_REMOVED = "Removed game lobby with id '{}' from active champ select lobbies. Current phase: '{}'.";
    public static final String CHAMPSELECT_INVALID_SPELL = "'{}' is not a valid summoner spell.";
    public static final String CHAMPSELECT_INVALID_SKIN = "'{}' is either not an owned skin or an invalid skin.";
    public static final String CHAMPSELECT_BAN_FORBIDDEN = "You're not allowed to ban or this champion was already banned.";
    public static final String CHAMPSELECT_PICK_FORBIDDEN = "You're not allowed to pick or this champion was already picked.";
    public static final String CHAMPSELECT_USER_NOT_FOUND = "User was not found in this game. This should not happen.";
    public static final String CHAMPSELECT_MESSAGE_FORBIDDEN = "This is not a valid message for the lobby type of the game lobby.";
    public static final String CHAMPSELECT_TRADE_NOT_POSSIBLE = "Trade is not possible, because the phase is not PRE_START or because one if the players is already trading.";
    public static final String CHAMPSELECT_DODGED = "User '{}' dodged the champselect with id '{}'. Their dodge timer is now '{}' seconds.";
    public static final String CHAMPSELECT_GAME_STARTED = "Game lobby with id '{}' started a new game server on port '{}'. Will kill their session in '{}' minutes.";
    public static final String BUGREPORT_ADDED = "User '{}' added a new bug report with the description '{}'.";
    public static final String BUGREPORT_TOO_MANY = "User '{}' already added 50 bug reports. This is the current maximum to prevent spam.";
    public static final String MAIL_ERROR = "Mail to '{}' with subject '{}' could not be sent. Underlying error is: ";
    public static final String NEWS_NOT_FOUND = "Could not find news article with id '{}'.";
    public static final String ALERT_NOT_FOUND = "Could not find alert with id '{}'.";
    public static final String SHOP_NO_MONEY = "User has not enough coins to purchase this item.";

    private LogMessage() {
    }
}
