using System.Runtime.Serialization;

namespace Fcl.Net.Core
{
    public enum FclServiceType
    {
        [EnumMember(Value = "authn")]
        Authn,

        [EnumMember(Value = "authz")]
        Authz,

        [EnumMember(Value = "pre-authz")]
        PreAuthz,

        [EnumMember(Value = "user-signature")]
        UserSignature,

        [EnumMember(Value = "back-channel-rpc")]
        BackChannel,

        [EnumMember(Value = "local-view")]
        LocalView,

        [EnumMember(Value = "open-id")]
        OpenID,

        [EnumMember(Value = "account-proof")]
        AccountProof,

        [EnumMember(Value = "authn-refresh")]
        AuthnRefresh
    }

    public enum ResponseStatus
    {
        [EnumMember(Value = "PENDING")]
        Pending,

        [EnumMember(Value = "APPROVED")]
        Approved,

        [EnumMember(Value = "DECLINED")]
        Declined,

        [EnumMember(Value = "REDIRECT")]
        Redirect
    }

    public enum FclServiceMethod
    {
        [EnumMember(Value = "HTTP/POST")]
        HttpPost,

        [EnumMember(Value = "HTTP/GET")]
        HttpGet,

        [EnumMember(Value = "IFRAME/RPC")]
        IFrameRPC,

        [EnumMember(Value = "POP/RPC")]
        PopRpc,
        
        [EnumMember(Value = "EXT/RPC")]
        ExtRpc,

        [EnumMember(Value = "DATA")]
        Data,

        //Views
        [EnumMember(Value = "VIEW/IFRAME")]
        ViewIFrame,

        [EnumMember(Value = "VIEW/POP")]
        ViewPop,

        [EnumMember(Value = "BROWSER/IFRAME")]
        BrowserIframe
    }

    public enum ResultType
    {
        Success,
        HttpError,
        UserCancel,
        Timeout,
        UnknownError
    }

    public enum FclInteractionTag
    {
        [EnumMember(Value = "UNKNOWN")]
        Unknown,

        [EnumMember(Value = "SCRIPT")]
        Script,

        [EnumMember(Value = "TRANSACTION")]
        Transaction,

        [EnumMember(Value = "GET_TRANSACTION_STATUS")]
        GetTransactionStatus,

        [EnumMember(Value = "GET_ACCOUNT")]
        GetAccount,

        [EnumMember(Value = "GET_EVENTS")]
        GetEvents,

        [EnumMember(Value = "GET_LATEST_BLOCK")]
        GetLatestBlock,

        [EnumMember(Value = "PING")]
        Ping,

        [EnumMember(Value = "GET_TRANSACTION")]
        GetTransaction,

        [EnumMember(Value = "GET_BLOCK_BY_ID")]
        GetBlockById,

        [EnumMember(Value = "GET_BLOCK_BY_HEIGHT")]
        GetBlockByHeight,

        [EnumMember(Value = "GET_BLOCK")]
        GetBlock,

        [EnumMember(Value = "GET_BLOCK_HEADER")]
        GetBlockHeader,

        [EnumMember(Value = "GET_COLLECTION")]
        GetCollection
    }

    public enum ChainId
    {
        Testnet,
        Mainnet
    }
}
