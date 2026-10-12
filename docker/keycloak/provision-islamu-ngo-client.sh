# ABOUTME: One-shot additive Keycloak provisioner for ISLAMU NGO first-party clients.
# ABOUTME: Reconciles only the NGO Blazor BFF client, API audience client, and audience mapper.

#!/usr/bin/env bash

set -euo pipefail

umask 077

OWNED_NGO_BLAZOR_CLIENT_ID="islamu-ngo-blazor"
OWNED_NGO_API_CLIENT_ID="islamu-ngo-api"
OWNED_NGO_AUDIENCE_MAPPER_NAME="islamu-ngo-api-audience"
KEYCLOAK_INTERNAL_URL="${KEYCLOAK_INTERNAL_URL:-http://keycloak:8080}"
KEYCLOAK_ALLOW_INSECURE_HTTP="${KEYCLOAK_ALLOW_INSECURE_HTTP:-false}"
KEYCLOAK_REALM="${KEYCLOAK_REALM:-ISLAMU}"
KEYCLOAK_ADMIN_REALM="${KEYCLOAK_ADMIN_REALM:-master}"
KEYCLOAK_ADMIN="${KEYCLOAK_ADMIN:-}"
KEYCLOAK_ADMIN_PASSWORD="${KEYCLOAK_ADMIN_PASSWORD:-}"
KEYCLOAK_ADMIN_SERVICE_CLIENT_ID="${KEYCLOAK_ADMIN_SERVICE_CLIENT_ID:-}"
KEYCLOAK_ADMIN_SERVICE_CLIENT_SECRET="${KEYCLOAK_ADMIN_SERVICE_CLIENT_SECRET:-}"
KEYCLOAK_ISLAMU_NGO_PROVISIONING_REQUIRED="${KEYCLOAK_ISLAMU_NGO_PROVISIONING_REQUIRED:-false}"
KEYCLOAK_ISLAMU_NGO_BLAZOR_CLIENT_ID="${KEYCLOAK_ISLAMU_NGO_BLAZOR_CLIENT_ID:-$OWNED_NGO_BLAZOR_CLIENT_ID}"
KEYCLOAK_ISLAMU_NGO_API_CLIENT_ID="${KEYCLOAK_ISLAMU_NGO_API_CLIENT_ID:-$OWNED_NGO_API_CLIENT_ID}"
KEYCLOAK_ISLAMU_NGO_AUDIENCE_MAPPER_NAME="${KEYCLOAK_ISLAMU_NGO_AUDIENCE_MAPPER_NAME:-$OWNED_NGO_AUDIENCE_MAPPER_NAME}"
KEYCLOAK_ISLAMU_NGO_BLAZOR_CLIENT_SECRET="${KEYCLOAK_ISLAMU_NGO_BLAZOR_CLIENT_SECRET:-}"
KEYCLOAK_ISLAMU_NGO_REDIRECT_URIS="${KEYCLOAK_ISLAMU_NGO_REDIRECT_URIS:-[\"http://localhost:17002/*\",\"https://localhost:7177/*\",\"https://100.64.0.2:7177/*\",\"http://localhost/*\",\"https://localhost/*\",\"http://127.0.0.1/*\",\"https://100.64.0.2/*\",\"http://100.64.0.2/*\",\"http://admin.localhost:17002/*\",\"http://admin.localhost/*\",\"https://admin.localhost/*\",\"http://host.docker.internal/*\"]}"
KEYCLOAK_ISLAMU_NGO_WEB_ORIGINS="${KEYCLOAK_ISLAMU_NGO_WEB_ORIGINS:-[\"+\"]}"
KEYCLOAK_ISLAMU_NGO_LOGOUT_REDIRECT_URIS="${KEYCLOAK_ISLAMU_NGO_LOGOUT_REDIRECT_URIS:-[\"http://localhost:17002/*\",\"https://localhost:7177/*\",\"http://localhost/*\",\"https://localhost/*\",\"https://100.64.0.2:7177/*\",\"https://100.64.0.2/*\",\"http://100.64.0.2:7177/*\",\"http://100.64.0.2/*\",\"http://admin.localhost:17002/*\",\"http://admin.localhost/*\",\"https://admin.localhost/*\"]}"
KCADM="${KCADM:-/opt/keycloak/bin/kcadm.sh}"
KCADM_CONFIG_DIR="$(mktemp -d)"
KCADM_CONFIG="$KCADM_CONFIG_DIR/kcadm.config"
SECRET_PATCH=""
LOGOUT_REDIRECT_URIS_ATTRIBUTE=""

export KCADM_CONFIG

log() {
  printf '[islamu-ngo-keycloak-provisioner] %s\n' "$1" >&2
}

fail() {
  log "ERROR: $1"
  exit 1
}

require_non_empty() {
  local name="$1"
  local value="$2"

  if [ -z "$value" ]; then
    fail "$name is required. Set it through environment or secret-provider configuration."
  fi
}

validate_identifier() {
  local name="$1"
  local value="$2"

  require_non_empty "$name" "$value"
  if [[ ! "$value" =~ ^[A-Za-z0-9._-]+$ ]]; then
    fail "$name may contain only letters, numbers, dot, underscore, and dash."
  fi
}

reject_reserved_owned_name() {
  local name="$1"
  local value="$2"

  case "$value" in
    islamu-event-blazor|islamu-event-api|islamu-event-api-audience)
      fail "$name must stay within the ISLAMU NGO-owned Keycloak resources."
      ;;
  esac
}

require_exact_owned_identifier() {
  local name="$1"
  local value="$2"
  local expected="$3"

  if [ "$value" != "$expected" ]; then
    fail "$name is immutable for the ISLAMU NGO provisioner and must be '$expected'. Refusing override/collision value '$value'."
  fi
}

validate_owned_resource_names() {
  require_exact_owned_identifier "KEYCLOAK_ISLAMU_NGO_BLAZOR_CLIENT_ID" "$KEYCLOAK_ISLAMU_NGO_BLAZOR_CLIENT_ID" "$OWNED_NGO_BLAZOR_CLIENT_ID"
  require_exact_owned_identifier "KEYCLOAK_ISLAMU_NGO_API_CLIENT_ID" "$KEYCLOAK_ISLAMU_NGO_API_CLIENT_ID" "$OWNED_NGO_API_CLIENT_ID"
  require_exact_owned_identifier "KEYCLOAK_ISLAMU_NGO_AUDIENCE_MAPPER_NAME" "$KEYCLOAK_ISLAMU_NGO_AUDIENCE_MAPPER_NAME" "$OWNED_NGO_AUDIENCE_MAPPER_NAME"

  reject_reserved_owned_name "KEYCLOAK_ISLAMU_NGO_BLAZOR_CLIENT_ID" "$KEYCLOAK_ISLAMU_NGO_BLAZOR_CLIENT_ID"
  reject_reserved_owned_name "KEYCLOAK_ISLAMU_NGO_API_CLIENT_ID" "$KEYCLOAK_ISLAMU_NGO_API_CLIENT_ID"
  reject_reserved_owned_name "KEYCLOAK_ISLAMU_NGO_AUDIENCE_MAPPER_NAME" "$KEYCLOAK_ISLAMU_NGO_AUDIENCE_MAPPER_NAME"

  if [ "$KEYCLOAK_ISLAMU_NGO_BLAZOR_CLIENT_ID" = "$KEYCLOAK_ISLAMU_NGO_API_CLIENT_ID" ]; then
    fail "KEYCLOAK_ISLAMU_NGO_BLAZOR_CLIENT_ID and KEYCLOAK_ISLAMU_NGO_API_CLIENT_ID must be distinct."
  fi
}

validate_boolean() {
  local name="$1"
  local value="$2"

  require_non_empty "$name" "$value"
  if [ "$value" != "true" ] && [ "$value" != "false" ]; then
    fail "$name must be exactly true or false."
  fi
}

validate_internal_url() {
  local value="$1"
  local authority

  require_non_empty "KEYCLOAK_INTERNAL_URL" "$value"
  if [[ "$value" =~ [[:cntrl:]] ]]; then
    fail "KEYCLOAK_INTERNAL_URL must not contain control characters."
  fi
  if [[ ! "$value" =~ ^https?:// ]]; then
    fail "KEYCLOAK_INTERNAL_URL must be an absolute http or https URL."
  fi
  if [[ "$value" == *\?* ]] || [[ "$value" == *\#* ]]; then
    fail "KEYCLOAK_INTERNAL_URL must not include query or fragment components."
  fi
  authority="${value#*://}"
  authority="${authority%%/*}"
  if [[ "$authority" == *@* ]]; then
    fail "KEYCLOAK_INTERNAL_URL must not include credentials."
  fi
  if [[ "$value" == http://* ]] && [ "$KEYCLOAK_ALLOW_INSECURE_HTTP" != "true" ]; then
    fail "KEYCLOAK_INTERNAL_URL uses HTTP; set KEYCLOAK_ALLOW_INSECURE_HTTP=true only for trusted local networks."
  fi
}

validate_secret() {
  local name="$1"
  local value="$2"

  require_non_empty "$name" "$value"
  if [ "${#value}" -lt 16 ]; then
    fail "$name must be at least 16 characters."
  fi
  if [ "${#value}" -gt 2048 ]; then
    fail "$name must be at most 2048 characters."
  fi
  if [[ "$value" =~ [[:cntrl:]] ]]; then
    fail "$name must contain only printable characters."
  fi
  case "${value,,}" in
    admin|password|secret|changeme|change-me|default|sample|example|test|keycloak|islamu-event-blazor-secret|islamu-event-api-secret|islamu-ngo-blazor-secret)
      fail "$name must not use a known sample, sentinel, or default value."
      ;;
  esac
}

validate_url_value() {
  local name="$1"
  local value="$2"
  local authority

  if [[ "$value" =~ [[:cntrl:]] ]]; then
    fail "$name must not contain control characters."
  fi
  if [[ ! "$value" =~ ^https?:// ]]; then
    fail "$name must contain only http or https URLs."
  fi
  authority="${value#*://}"
  authority="${authority%%/*}"
  if [[ "$authority" == *@* ]]; then
    fail "$name must not include URL credentials."
  fi
}

consume_json_string_array() {
  local name="$1"
  local value="$2"
  local callback="$3"
  local inner
  local item

  require_non_empty "$name" "$value"
  if [[ "$value" =~ [[:cntrl:]] ]]; then
    fail "$name must not contain control characters."
  fi
  if [[ "$value" != \[* ]] || [[ "$value" != *\] ]]; then
    fail "$name must be a JSON array of strings."
  fi

  inner="${value:1:${#value}-2}"
  if [ -z "$inner" ]; then
    return 0
  fi

  while [ -n "$inner" ]; do
    inner="${inner#${inner%%[! ]*}}"
    [ -n "$inner" ] || break
    if [[ "$inner" != \"* ]]; then
      fail "$name must be a JSON array of strings."
    fi
    inner="${inner:1}"
    item=""

    while true; do
      if [ -z "$inner" ]; then
        fail "$name has an unterminated string value."
      fi
      case "${inner:0:1}" in
        '"')
          inner="${inner:1}"
          break
          ;;
        '\\')
          fail "$name must not use escaped string values."
          ;;
        *)
          item+="${inner:0:1}"
          inner="${inner:1}"
          ;;
      esac
    done

    "$callback" "$name" "$item"
    if [ -z "$inner" ]; then
      break
    fi
    inner="${inner#${inner%%[! ]*}}"
    if [ -z "$inner" ]; then
      break
    fi
    if [[ "$inner" != ,* ]]; then
      fail "$name must separate string values with commas."
    fi
    inner="${inner:1}"
  done
}

validate_json_url_array_item() {
  validate_url_value "$1" "$2"
}

validate_json_web_origin_item() {
  if [ "$2" = "+" ]; then
    return 0
  fi
  validate_url_value "$1" "$2"
}

append_logout_redirect_item() {
  validate_url_value "$1" "$2"
  if [ -z "$LOGOUT_REDIRECT_URIS_ATTRIBUTE" ]; then
    LOGOUT_REDIRECT_URIS_ATTRIBUTE="$2"
  else
    LOGOUT_REDIRECT_URIS_ATTRIBUTE="$LOGOUT_REDIRECT_URIS_ATTRIBUTE##$2"
  fi
}

validate_json_string_array() {
  local name="$1"
  local value="$2"
  local kind="$3"

  case "$kind" in
    url)
      consume_json_string_array "$name" "$value" validate_json_url_array_item
      ;;
    web-origin)
      consume_json_string_array "$name" "$value" validate_json_web_origin_item
      ;;
    logout-url)
      LOGOUT_REDIRECT_URIS_ATTRIBUTE=""
      consume_json_string_array "$name" "$value" append_logout_redirect_item
      ;;
    *)
      fail "Unknown JSON string-array validation kind '$kind'."
      ;;
  esac
}

json_escape() {
  local value="${1//\\/\\\\}"
  value="${value//\"/\\\"}"
  printf '%s' "$value"
}

cleanup() {
  if [ -n "${SECRET_PATCH:-}" ] && [ -f "$SECRET_PATCH" ]; then
    rm -f "$SECRET_PATCH"
  fi
  if [ -n "${KCADM_CONFIG_DIR:-}" ] && [ -d "$KCADM_CONFIG_DIR" ]; then
    rm -rf "$KCADM_CONFIG_DIR"
  fi
}

trap cleanup EXIT

kc() {
  local command="$1"
  shift
  "$KCADM" "$command" --config "$KCADM_CONFIG" "$@"
}

exact_resource_uuid() {
  local resource="$1"
  local query="$2"
  local field="$3"
  local expected="$4"
  local label="$5"
  local rows
  local row
  local id
  local actual
  local found=""
  local count

  rows="$(kc get "$resource" -r "$KEYCLOAK_REALM" -q "$query" --fields "id,$field" --format csv --noquotes | tr -d '\r')"
  count=0

  while IFS= read -r row; do
    [ -n "$row" ] || continue
    id="${row%%,*}"
    actual="${row#*,}"
    if [ "$actual" != "$expected" ]; then
      continue
    fi
    found="$id"
    count=$((count + 1))
  done <<ROWS
$rows
ROWS

  if [ "$count" -gt 1 ]; then
    fail "Duplicate Keycloak resources matched $label. Refusing to choose one."
  fi

  printf '%s\n' "$found"
}

client_uuid() {
  exact_resource_uuid clients "clientId=$1" clientId "$1" "client '$1'"
}

mapper_uuid() {
  local client_id="$1"
  local mapper_name="$2"
  local uuid

  uuid="$(client_uuid "$client_id")"
  require_non_empty "client uuid for $client_id" "$uuid"
  exact_resource_uuid "clients/$uuid/protocol-mappers/models" "name=$mapper_name" name "$mapper_name" "mapper '$mapper_name' on client '$client_id'"
}

write_secret_patch() {
  local secret="$1"

  SECRET_PATCH="$(mktemp "$KCADM_CONFIG_DIR/secret-patch.XXXXXX")"
  cat >"$SECRET_PATCH" <<JSON
{"secret":"$(json_escape "$secret")"}
JSON
}

ensure_api_client() {
  local uuid

  uuid="$(client_uuid "$KEYCLOAK_ISLAMU_NGO_API_CLIENT_ID")"
  if [ -z "$uuid" ]; then
    if ! kc create clients -r "$KEYCLOAK_REALM" \
      -s "clientId=$KEYCLOAK_ISLAMU_NGO_API_CLIENT_ID" \
      -s 'name=ISLAMU NGO API (Resource Server)' \
      -s 'description=Resource server for ISLAMU NGO API audience validation. No login flows.' \
      -s enabled=true \
      -s consentRequired=false \
      -s clientAuthenticatorType=client-secret \
      -s publicClient=false \
      -s bearerOnly=true \
      -s standardFlowEnabled=false \
      -s directAccessGrantsEnabled=false \
      -s serviceAccountsEnabled=false \
      -s implicitFlowEnabled=false \
      -s protocol=openid-connect \
      -s 'defaultClientScopes=["openid","profile","email","web-origins"]' >/dev/null; then
      log "API audience client create failed; re-querying exact owned client in case it was created_by_concurrent_run."
    fi
    uuid="$(client_uuid "$KEYCLOAK_ISLAMU_NGO_API_CLIENT_ID")"
    require_non_empty "created client uuid for $KEYCLOAK_ISLAMU_NGO_API_CLIENT_ID" "$uuid"
  fi

  kc update "clients/$uuid" -r "$KEYCLOAK_REALM" \
    -s 'name=ISLAMU NGO API (Resource Server)' \
    -s 'description=Resource server for ISLAMU NGO API audience validation. No login flows.' \
    -s enabled=true \
    -s consentRequired=false \
    -s clientAuthenticatorType=client-secret \
    -s publicClient=false \
    -s bearerOnly=true \
    -s standardFlowEnabled=false \
    -s directAccessGrantsEnabled=false \
    -s serviceAccountsEnabled=false \
    -s implicitFlowEnabled=false \
    -s protocol=openid-connect \
    -s 'defaultClientScopes=["openid","profile","email","web-origins"]' >/dev/null
  log "Reconciled API audience client '$KEYCLOAK_ISLAMU_NGO_API_CLIENT_ID'."
}

ensure_bff_client() {
  local uuid
  local attributes

  uuid="$(client_uuid "$KEYCLOAK_ISLAMU_NGO_BLAZOR_CLIENT_ID")"
  if [ -z "$uuid" ]; then
    if ! kc create clients -r "$KEYCLOAK_REALM" \
      -s "clientId=$KEYCLOAK_ISLAMU_NGO_BLAZOR_CLIENT_ID" \
      -s 'name=ISLAMU NGO Blazor (BFF)' \
      -s 'description=Confidential OIDC client for the ISLAMU NGO Blazor Server BFF. Authorization code flow with PKCE.' \
      -s enabled=true \
      -s consentRequired=false \
      -s clientAuthenticatorType=client-secret \
      -s publicClient=false \
      -s standardFlowEnabled=true \
      -s directAccessGrantsEnabled=false \
      -s serviceAccountsEnabled=false \
      -s implicitFlowEnabled=false \
      -s fullScopeAllowed=false \
      -s protocol=openid-connect >/dev/null; then
      log "BFF client create failed; re-querying exact owned client in case it was created_by_concurrent_run."
    fi
    uuid="$(client_uuid "$KEYCLOAK_ISLAMU_NGO_BLAZOR_CLIENT_ID")"
    require_non_empty "created client uuid for $KEYCLOAK_ISLAMU_NGO_BLAZOR_CLIENT_ID" "$uuid"
  fi

  attributes="{\"pkce.code.challenge.method\":\"S256\",\"post.logout.redirect.uris\":\"$LOGOUT_REDIRECT_URIS_ATTRIBUTE\",\"backchannel.logout.session.required\":\"true\",\"backchannel.logout.revoke.offline.tokens\":\"false\",\"use.refresh.tokens\":\"true\",\"oauth2.device.authorization.grant.enabled\":\"false\",\"oidc.ciba.grant.enabled\":\"false\"}"

  kc update "clients/$uuid" -r "$KEYCLOAK_REALM" \
    -s 'name=ISLAMU NGO Blazor (BFF)' \
    -s 'description=Confidential OIDC client for the ISLAMU NGO Blazor Server BFF. Authorization code flow with PKCE.' \
    -s enabled=true \
    -s consentRequired=false \
    -s clientAuthenticatorType=client-secret \
    -s publicClient=false \
    -s standardFlowEnabled=true \
    -s directAccessGrantsEnabled=false \
    -s serviceAccountsEnabled=false \
    -s implicitFlowEnabled=false \
    -s fullScopeAllowed=false \
    -s protocol=openid-connect \
    -s "redirectUris=$KEYCLOAK_ISLAMU_NGO_REDIRECT_URIS" \
    -s "webOrigins=$KEYCLOAK_ISLAMU_NGO_WEB_ORIGINS" \
    -s 'defaultClientScopes=["openid","profile","email","web-origins","acr"]' \
    -s 'optionalClientScopes=["offline_access","address","phone"]' \
    -s "attributes=$attributes" >/dev/null

  write_secret_patch "$KEYCLOAK_ISLAMU_NGO_BLAZOR_CLIENT_SECRET"
  kc update "clients/$uuid" -r "$KEYCLOAK_REALM" -f "$SECRET_PATCH" >/dev/null
  log "Reconciled confidential BFF client '$KEYCLOAK_ISLAMU_NGO_BLAZOR_CLIENT_ID' (secret redacted)."
}

ensure_mapper() {
  local client_id="$1"
  local client_uuid_value
  local mapper_config
  local uuid

  client_uuid_value="$(client_uuid "$client_id")"
  require_non_empty "client uuid for $client_id" "$client_uuid_value"
  uuid="$(mapper_uuid "$client_id" "$KEYCLOAK_ISLAMU_NGO_AUDIENCE_MAPPER_NAME")"
  printf -v mapper_config '{"included.client.audience":"%s","id.token.claim":"false","access.token.claim":"true"}' "$KEYCLOAK_ISLAMU_NGO_API_CLIENT_ID"

  if [ -z "$uuid" ]; then
    if ! kc create "clients/$client_uuid_value/protocol-mappers/models" -r "$KEYCLOAK_REALM" \
      -s "name=$KEYCLOAK_ISLAMU_NGO_AUDIENCE_MAPPER_NAME" \
      -s protocol=openid-connect \
      -s protocolMapper=oidc-audience-mapper \
      -s consentRequired=false \
      -s "config=$mapper_config" >/dev/null; then
      log "Audience mapper create failed; re-querying exact owned mapper in case it was created_by_concurrent_run."
    fi
    uuid="$(mapper_uuid "$client_id" "$KEYCLOAK_ISLAMU_NGO_AUDIENCE_MAPPER_NAME")"
    require_non_empty "created mapper uuid for $KEYCLOAK_ISLAMU_NGO_AUDIENCE_MAPPER_NAME" "$uuid"
  fi

  kc update "clients/$client_uuid_value/protocol-mappers/models/$uuid" -r "$KEYCLOAK_REALM" \
    -s "name=$KEYCLOAK_ISLAMU_NGO_AUDIENCE_MAPPER_NAME" \
    -s protocol=openid-connect \
    -s protocolMapper=oidc-audience-mapper \
    -s consentRequired=false \
    -s "config=$mapper_config" >/dev/null
  log "Reconciled audience mapper '$KEYCLOAK_ISLAMU_NGO_AUDIENCE_MAPPER_NAME' on '$client_id'."
}

authenticate() {
  if [ -n "$KEYCLOAK_ADMIN_SERVICE_CLIENT_ID" ] || [ -n "$KEYCLOAK_ADMIN_SERVICE_CLIENT_SECRET" ]; then
    require_non_empty "KEYCLOAK_ADMIN_SERVICE_CLIENT_ID" "$KEYCLOAK_ADMIN_SERVICE_CLIENT_ID"
    validate_secret "KEYCLOAK_ADMIN_SERVICE_CLIENT_SECRET" "$KEYCLOAK_ADMIN_SERVICE_CLIENT_SECRET"
    KC_CLI_CLIENT_SECRET="$KEYCLOAK_ADMIN_SERVICE_CLIENT_SECRET" "$KCADM" config credentials --config "$KCADM_CONFIG" \
      --server "$KEYCLOAK_INTERNAL_URL" \
      --realm "$KEYCLOAK_ADMIN_REALM" \
      --client "$KEYCLOAK_ADMIN_SERVICE_CLIENT_ID" >/dev/null
    return 0
  fi

  require_non_empty "KEYCLOAK_ADMIN" "$KEYCLOAK_ADMIN"
  validate_secret "KEYCLOAK_ADMIN_PASSWORD" "$KEYCLOAK_ADMIN_PASSWORD"
  KC_CLI_PASSWORD="$KEYCLOAK_ADMIN_PASSWORD" "$KCADM" config credentials --config "$KCADM_CONFIG" \
    --server "$KEYCLOAK_INTERNAL_URL" \
    --realm "$KEYCLOAK_ADMIN_REALM" \
    --user "$KEYCLOAK_ADMIN" >/dev/null
}

main() {
  validate_boolean "KEYCLOAK_ISLAMU_NGO_PROVISIONING_REQUIRED" "$KEYCLOAK_ISLAMU_NGO_PROVISIONING_REQUIRED"
  validate_boolean "KEYCLOAK_ALLOW_INSECURE_HTTP" "$KEYCLOAK_ALLOW_INSECURE_HTTP"

  if [ "$KEYCLOAK_ISLAMU_NGO_PROVISIONING_REQUIRED" = "false" ]; then
    log "KEYCLOAK_ISLAMU_NGO_PROVISIONING_REQUIRED=false; leaving realm unchanged."
    return 0
  fi

  if [ ! -x "$KCADM" ]; then
    fail "kcadm.sh was not found at '$KCADM'. Run this script inside the Keycloak image or set KCADM."
  fi

  validate_internal_url "$KEYCLOAK_INTERNAL_URL"
  validate_identifier "KEYCLOAK_REALM" "$KEYCLOAK_REALM"
  validate_identifier "KEYCLOAK_ADMIN_REALM" "$KEYCLOAK_ADMIN_REALM"
  validate_identifier "KEYCLOAK_ISLAMU_NGO_BLAZOR_CLIENT_ID" "$KEYCLOAK_ISLAMU_NGO_BLAZOR_CLIENT_ID"
  validate_identifier "KEYCLOAK_ISLAMU_NGO_API_CLIENT_ID" "$KEYCLOAK_ISLAMU_NGO_API_CLIENT_ID"
  validate_identifier "KEYCLOAK_ISLAMU_NGO_AUDIENCE_MAPPER_NAME" "$KEYCLOAK_ISLAMU_NGO_AUDIENCE_MAPPER_NAME"
  validate_owned_resource_names
  validate_json_string_array "KEYCLOAK_ISLAMU_NGO_REDIRECT_URIS" "$KEYCLOAK_ISLAMU_NGO_REDIRECT_URIS" url
  validate_json_string_array "KEYCLOAK_ISLAMU_NGO_WEB_ORIGINS" "$KEYCLOAK_ISLAMU_NGO_WEB_ORIGINS" web-origin
  validate_json_string_array "KEYCLOAK_ISLAMU_NGO_LOGOUT_REDIRECT_URIS" "$KEYCLOAK_ISLAMU_NGO_LOGOUT_REDIRECT_URIS" logout-url
  validate_secret "KEYCLOAK_ISLAMU_NGO_BLAZOR_CLIENT_SECRET" "$KEYCLOAK_ISLAMU_NGO_BLAZOR_CLIENT_SECRET"

  log "Authenticating to Keycloak Admin API at ${KEYCLOAK_INTERNAL_URL} for admin realm '${KEYCLOAK_ADMIN_REALM}' and target realm '${KEYCLOAK_REALM}'."
  authenticate

  kc get "realms/$KEYCLOAK_REALM" >/dev/null || fail "Realm '$KEYCLOAK_REALM' was not found. Import the shared realm before provisioning NGO clients."

  ensure_api_client
  ensure_bff_client
  ensure_mapper "$KEYCLOAK_ISLAMU_NGO_BLAZOR_CLIENT_ID"

  log "ISLAMU NGO Keycloak client provisioning completed."
}

main "$@"
