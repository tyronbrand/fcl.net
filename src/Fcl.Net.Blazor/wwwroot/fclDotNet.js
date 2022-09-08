export function execPop(service, body, config) {
    return new Promise((resolve, reject) => {       

        pop(service, {
            async onReady(_, { send }) {
                try {
                    send({
                        type: "FCL:VIEW:READY:RESPONSE",
                        body,
                        service: {
                            params: service.params,
                            data: service.data,
                            type: service.type,
                        },
                        config,
                    })
                    send({
                        type: "FCL:FRAME:READY:RESPONSE",
                        body,
                        service: {
                            params: service.params,
                            data: service.data,
                            type: service.type,
                        },
                        config,
                        deprecated: {
                            message:
                                "FCL:FRAME:READY:RESPONSE is deprecated and replaced with type: FCL:VIEW:READY:RESPONSE",
                        },
                    })
                    //if (includeOlderJsonRpcCall) {
                    //    send({
                    //        jsonrpc: "2.0",
                    //        id: id,
                    //        method: "fcl:sign",
                    //        params: [body, service.params],
                    //    })
                    //}
                } catch (error) {
                    throw error
                }
            },

            onResponse(e, { close }) {
                try {
                    if (typeof e.data !== "object") return
                    const resp = normalizePollingResponse(e.data)
                    console.log(resp)


                    switch (resp.status) {
                        case "APPROVED":
                            resolve(JSON.stringify(resp.data))
                            close()
                            break

                        case "DECLINED":
                            reject(`Declined: ${resp.reason || "No reason supplied"}`)
                            close()
                            break

                        case "REDIRECT":
                            resolve(JSON.stringify(resp))
                            close()
                            break

                        default:
                            reject(`Declined: No reason supplied`)
                            close()
                            break
                    }
                } catch (error) {
                    console.error("execPopRPC onResponse error", error)
                    throw error
                }
            },

            onMessage(e, { close }) {
                try {
                    if (typeof e.data !== "object") return
                    if (e.data.jsonrpc !== "2.0") return
                    if (e.data.id !== id) return
                    const resp = normalizePollingResponse(e.data.result)

                    switch (resp.status) {
                        case "APPROVED":
                            resolve(JSON.stringify(resp.data))
                            close()
                            break

                        case "DECLINED":
                            reject(`Declined: ${resp.reason || "No reason supplied"}`)
                            close()
                            break

                        case "REDIRECT":
                            resolve(JSON.stringify(resp))
                            close()
                            break

                        default:
                            reject(`Declined: No reason supplied`)
                            close()
                            break
                    }
                } catch (error) {
                    console.error("execPopRPC onMessage error", error)
                    throw error
                }
            },

            onClose() {
                reject(`Declined: Externally Halted`)
            },
        })
    })
}

export function pop(service, opts = {}) {
    if (service == null) return { send: noop, close: noop }

    const onClose = opts.onClose || noop
    const onMessage = opts.onMessage || noop
    const onReady = opts.onReady || noop
    const onResponse = opts.onResponse || noop

    const handler = buildMessageHandler({
        close,
        send,
        onReady,
        onResponse,
        onMessage,
    })
    window.addEventListener("message", handler)

    const [$pop, unmount] = renderPop(serviceEndpoint(service))

    const timer = setInterval(function () {
        if ($pop && $pop.closed) {
            close()
        }
    }, 500)

    return { send, close }   

    function close() {
        try {
            window.removeEventListener("message", handler)
            clearInterval(timer)
            unmount()
            onClose()
        } catch (error) {
            console.error("Popup Close Error", error)
        }
    }

    function send(msg) {
        try {
            $pop.postMessage(JSON.parse(JSON.stringify(msg || {})), "*")
        } catch (error) {
            console.error("Popup Send Error", msg, error)
        }
    }
}

const POP = "FCL_POP"

let popup = null
let previousUrl = null

function popupWindow(url, windowName, win, w, h) {
    const y = win.top.outerHeight / 2 + win.top.screenY - h / 2
    const x = win.top.outerWidth / 2 + win.top.screenX - w / 2
    const popup = win.open(
        url,
        windowName,
        `toolbar=no, location=no, directories=no, status=no, menubar=no, scrollbars=no, resizable=no, copyhistory=no, width=${w}, height=${h}, top=${y}, left=${x}`
    )
    if (!popup)
        throw new Error("Popup failed to open (was it blocked by a popup blocker?)")
    return popup
}

export function renderPop(src, returnDotNetObj) {
    if (popup == null || popup?.closed) {
        popup = popupWindow(src, POP, window, 640, 770)
    } else if (previousUrl !== src) {
        popup.location.replace(src)
        popup.focus()
    } else {
        popup.focus()
    }

    previousUrl = src

    const unmount = () => {
        if (popup && !popup.closed) {
            popup.close()
        }
        popup = null
    }

    if (returnDotNetObj) {
        return {
            close : unmount
        }
    }

    return [popup, unmount]
}

export function execIFrame(service, body, config) {
    return new Promise((resolve, reject) => {
        frame(service, {
            async onReady(_, { send }) {
                try {
                    send({
                        type: "FCL:VIEW:READY:RESPONSE",
                        body,
                        service: {
                            params: service.params,
                            data: service.data,
                            type: service.type,
                        },
                        config,
                    })
                    send({
                        type: "FCL:FRAME:READY:RESPONSE",
                        body,
                        service: {
                            params: service.params,
                            data: service.data,
                            type: service.type,
                        },
                        config,
                        deprecated: {
                            message:
                                "FCL:FRAME:READY:RESPONSE is deprecated and replaced with type: FCL:VIEW:READY:RESPONSE",
                        },
                    })
                } catch (error) {
                    throw error
                }
            },

            onResponse(e, { close }) {
                try {
                    if (typeof e.data !== "object") return
                    const resp = normalizePollingResponse(e.data)
                    console.log('onResponse')
                    console.log(resp)

                    switch (resp.status) {
                        case "APPROVED":
                            resolve(JSON.stringify(resp))
                            close()
                            break

                        case "DECLINED":
                            reject(`Declined: ${resp.reason || "No reason supplied"}`)
                            close()
                            break

                        case "REDIRECT":
                            resolve(JSON.stringify(resp))
                            close()
                            break

                        default:                            
                            reject(`Declined: No reason supplied`)
                            close()
                            break
                    }
                } catch (error) {
                    console.error("execIframeRPC onResponse error", error)
                    throw error
                }
            },

            onMessage(e, { close }) {
                try {
                    if (typeof e.data !== "object") return
                    if (e.data.jsonrpc !== "2.0") return
                    if (e.data.id !== id) return
                    const resp = normalizePollingResponse(e.data.result)
                    console.log(resp)

                    switch (resp.status) {
                        case "APPROVED":
                            resolve(JSON.stringify(resp.data))
                            close()
                            break

                        case "DECLINED":
                            reject(`Declined: ${resp.reason || "No reason supplied"}`)
                            close()
                            break

                        case "REDIRECT":
                            resolve(JSON.stringify(resp))
                            close()
                            break

                        default:
                            reject(`Declined: No reason supplied`)
                            close()
                            break
                    }
                } catch (error) {
                    console.error("execIframeRPC onMessage error", error)
                    throw error
                }
            },

            onClose() {
                reject(`Declined: Externally Halted`)
            },
        })        
    })
}

export function frame(service, opts = {}) {
    if (service == null) return { send: noop, close: noop }

    const onClose = opts.onClose || noop
    const onMessage = opts.onMessage || noop
    const onReady = opts.onReady || noop
    const onResponse = opts.onResponse || noop

    const handler = buildMessageHandler({
        close,
        send,
        onReady,
        onResponse,
        onMessage,
    })
    window.addEventListener("message", handler)

    const [$frame, unmount] = renderFrame(serviceEndpoint(service))    

    return { send, close }

    function close() {
        try {
            window.removeEventListener("message", handler)
            unmount()
            onClose()
        } catch (error) {
            console.error("Frame Close Error", error)
        }
    }

    function send(msg) {
        try {
            $frame.postMessage(JSON.parse(JSON.stringify(msg || {})), "*")
        } catch (error) {
            console.error("Frame Send Error", msg, error)
        }
    }
}

const FRAME = "FCL_IFRAME"

const FRAME_STYLES = `
  position:fixed;
  top: 0px;
  right: 0px;
  bottom: 0px;
  left: 0px;
  height: 100%;
  width: 100vw;
  display:block;
  background:rgba(0,0,0,0.25);
  z-index: 2147483647;
  box-sizing: border-box;
`

export function renderFrame(src, returnDotNetObj) {
    const $frame = document.createElement("iframe")
    $frame.src = src
    $frame.id = FRAME
    $frame.allow = "usb *; hid *"
    $frame.frameBorder = "0"
    $frame.style.cssText = FRAME_STYLES
    document.body.append($frame)

    const unmount = () => {
        if (document.getElementById(FRAME)) {
            document.getElementById(FRAME).remove()
        }
    }

    if (returnDotNetObj) {
        return {
            close: unmount
        }
    }

    return [$frame.contentWindow, unmount]
}

const CLOSE_EVENT = "FCL:VIEW:CLOSE"
const READY_EVENT = "FCL:VIEW:READY"
const RESPONSE_EVENT = "FCL:VIEW:RESPONSE"

const _ = e => typeof e === "string" && e.toLowerCase()

const IGNORE = new Set([
    "monetizationstart",
    "monetizationpending",
    "monetizationprogress",
    "monetizationstop",
])

const deprecate = (was, want) =>
    console.warn(
        "DEPRECATION NOTICE",
        `Received ${was}, please use ${want} for this and future versions of FCL`
    )

export const buildMessageHandler = ({ close, send, onReady, onResponse, onMessage }) => e =>
{
    try {
        if (typeof e.data !== "object") return
        if (IGNORE.has(e.data.type)) return
        if (_(e.data.type) === _(CLOSE_EVENT)) close()
        if (_(e.data.type) === _(READY_EVENT)) onReady(e, { send, close })
        if (_(e.data.type) === _(RESPONSE_EVENT)) onResponse(e, { send, close })
        onMessage(e, { send, close })

        // Backwards Compatible
        if (_(e.data.type) === _("FCL:FRAME:READY")) {
            deprecate(e.data.type, READY_EVENT)
            onReady(e, { send, close })
        }
        if (_(e.data.type) === _("FCL:FRAME:RESPONSE")) {
            deprecate(e.data.type, RESPONSE_EVENT)
            onResponse(e, { send, close })
        }
        if (_(e.data.type) === _("FCL:FRAME:CLOSE")) {
            deprecate(e.data.type, CLOSE_EVENT)
            close()
        }        
        if (_(e.data.type) === _("FCL::CHALLENGE::RESPONSE")) {
            deprecate(e.data.type, RESPONSE_EVENT)
            onResponse(e, { send, close })
        }
        if (_(e.data.type) === _("FCL::AUTHZ_READY")) {
            deprecate(e.data.type, READY_EVENT)
            onReady(e, { send, close })
        }
        if (_(e.data.type) === _("FCL::CHALLENGE::CANCEL")) {
            deprecate(e.data.type, CLOSE_EVENT)
            close()
        }
        if (_(e.data.type) === _("FCL::CANCEL")) {
            deprecate(e.data.type, CLOSE_EVENT)
            close()
        }
    } catch (error) {
        console.error("Frame Callback Error", error)
        close()
    }
}

export function serviceEndpoint(service) {
    const url = new URL(service.endpoint)
    url.searchParams.append("l6n", window.location.origin)
    if (service.params != null) {
        for (let [key, value] of Object.entries(service.params || {})) {
            url.searchParams.append(key, value)
        }
    }
    return url
}

export const SERVICE_PRAGMA = {
    f_type: "Service",
    f_vsn: "1.0.0",
}

export const IDENTITY_PRAGMA = {
    f_type: "Identity",
    f_vsn: "1.0.0",
}

export const PROVIDER_PRAGMA = {
    f_type: "Provider",
    f_vsn: "1.0.0",
}

export const USER_PRAGMA = {
    f_type: "USER",
    f_vsn: "1.0.0",
}

export const POLLING_RESPONSE_PRAGMA = {
    f_type: "PollingResponse",
    f_vsn: "1.0.0",
}

export const COMPOSITE_SIGNATURE_PRAGMA = {
    f_type: "CompositeSignature",
    f_vsn: "1.0.0",
}

export const OPEN_ID_PRAGMA = {
    f_type: "OpenId",
    f_vsn: "1.0.0",
}

export function normalizeBackChannelRpc(service) {
    if (service == null) return null

    switch (service["f_vsn"]) {
        case "1.0.0":
            return service

        default:
            return {
                ...SERVICE_PRAGMA,
                type: "back-channel-rpc",
                endpoint: service.endpoint,
                method: service.method,
                params: service.params || {},
                data: service.data || {},
            }
    }
}

export function normalizeFrame(service) {
    if (service == null) return null

    switch (service["f_vsn"]) {
        case "1.0.0":
            return service

        default:
            return {
                old: service,
                ...SERVICE_PRAGMA,
                type: "frame",
                endpoint: service.endpoint,
                params: service.params || {},
                data: service.data || {},
            }
    }
}

export function normalizePollingResponse(resp) {
    if (resp == null) return null

    switch (resp["f_vsn"]) {
        case "1.0.0":
            return resp

        default:
            return {
                ...POLLING_RESPONSE_PRAGMA,
                status: resp.status ?? "APPROVED",
                reason: resp.reason ?? null,
                data: resp.compositeSignature || resp.data || { ...resp } || {},
                updates: normalizeBackChannelRpc(resp.authorizationUpdates),
                local: normalizeFrame((resp.local || [])[0]),
            }
    }
}