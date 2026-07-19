# ABOUTME: Validates production environment variables without printing secret or URL values.
# ABOUTME: Rejects sample, local, and malformed runtime configuration before deployment.
set -eu

fail() {
  echo "production validation error: $1"
  exit 1
}

read_var() {
  case "$1" in
    POSTGRESQL_PASSWORD) printf '%s' "${POSTGRESQL_PASSWORD:-}" ;;
    KEYCLOAK_ADMIN_SERVICE_CLIENT_SECRET) printf '%s' "${KEYCLOAK_ADMIN_SERVICE_CLIENT_SECRET:-}" ;;
    KEYCLOAK_ISLAMU_NGO_BLAZOR_CLIENT_SECRET) printf '%s' "${KEYCLOAK_ISLAMU_NGO_BLAZOR_CLIENT_SECRET:-}" ;;
    SETUP_SECRET) printf '%s' "${SETUP_SECRET:-}" ;;
    CACHE_CONNECTION_STRING) printf '%s' "${CACHE_CONNECTION_STRING:-}" ;;
    ISLAMU_API_PUBLIC_URL) printf '%s' "${ISLAMU_API_PUBLIC_URL:-}" ;;
    ISLAMU_BFF_PUBLIC_URL) printf '%s' "${ISLAMU_BFF_PUBLIC_URL:-}" ;;
    KEYCLOAK_AUTHORITY) printf '%s' "${KEYCLOAK_AUTHORITY:-}" ;;
    KEYCLOAK_INTERNAL_URL) printf '%s' "${KEYCLOAK_INTERNAL_URL:-}" ;;
    KEYCLOAK_REALM) printf '%s' "${KEYCLOAK_REALM:-}" ;;
    KEYCLOAK_ISLAMU_NGO_BLAZOR_CLIENT_ID) printf '%s' "${KEYCLOAK_ISLAMU_NGO_BLAZOR_CLIENT_ID:-}" ;;
    KEYCLOAK_ISLAMU_NGO_API_CLIENT_ID) printf '%s' "${KEYCLOAK_ISLAMU_NGO_API_CLIENT_ID:-}" ;;
    KEYCLOAK_ISLAMU_NGO_AUDIENCE_MAPPER_NAME) printf '%s' "${KEYCLOAK_ISLAMU_NGO_AUDIENCE_MAPPER_NAME:-}" ;;
    *) fail "$1" ;;
  esac
}

reject_placeholder() {
  name="$1"
  value=$(read_var "$name")
  lower=$(printf '%s' "$value" | tr '[:upper:]' '[:lower:]')

  [ -n "$value" ] || fail "$name"

  case "$lower" in
    *sample*|*sentinel*|*fixture*|*local*|*localhost*|*127.0.0.1*|*.test*|*example*|*changeme*|*password*|*default*|*dummy*|*placeholder*)
      fail "$name"
      ;;
  esac
}

validate_secret() {
  name="$1"
  reject_placeholder "$name"
  value=$(read_var "$name")

  [ "${#value}" -ge 16 ] || fail "$name"
}

validate_https_url() {
  name="$1"
  reject_placeholder "$name"
  value=$(read_var "$name")

  case "$value" in
    https://*) ;;
    *) fail "$name" ;;
  esac

  without_scheme=${value#https://}
  [ -n "$without_scheme" ] || fail "$name"
  case "$without_scheme" in
    /*|*@*|*\?*|*#*|*//*) fail "$name" ;;
  esac

  authority=${without_scheme%%/*}
  [ -n "$authority" ] || fail "$name"
  case "$authority" in
    :*) fail "$name" ;;
  esac
}

validate_exact() {
  name="$1"
  expected="$2"
  value=$(read_var "$name")

  [ "$value" = "$expected" ] || fail "$name"
}

require_distinct_secret() {
  current_name="$1"
  current_value=$(read_var "$current_name")
  shift

  for previous_name in "$@"; do
    previous_value=$(read_var "$previous_name")
    [ "$current_value" != "$previous_value" ] || fail "$current_name"
  done
}

validate_secret POSTGRESQL_PASSWORD
validate_secret KEYCLOAK_ADMIN_SERVICE_CLIENT_SECRET
validate_secret KEYCLOAK_ISLAMU_NGO_BLAZOR_CLIENT_SECRET
validate_secret SETUP_SECRET
validate_secret CACHE_CONNECTION_STRING

require_distinct_secret KEYCLOAK_ADMIN_SERVICE_CLIENT_SECRET POSTGRESQL_PASSWORD
require_distinct_secret KEYCLOAK_ISLAMU_NGO_BLAZOR_CLIENT_SECRET POSTGRESQL_PASSWORD KEYCLOAK_ADMIN_SERVICE_CLIENT_SECRET
require_distinct_secret SETUP_SECRET POSTGRESQL_PASSWORD KEYCLOAK_ADMIN_SERVICE_CLIENT_SECRET KEYCLOAK_ISLAMU_NGO_BLAZOR_CLIENT_SECRET
require_distinct_secret CACHE_CONNECTION_STRING POSTGRESQL_PASSWORD KEYCLOAK_ADMIN_SERVICE_CLIENT_SECRET KEYCLOAK_ISLAMU_NGO_BLAZOR_CLIENT_SECRET SETUP_SECRET

validate_https_url ISLAMU_API_PUBLIC_URL
validate_https_url ISLAMU_BFF_PUBLIC_URL
validate_https_url KEYCLOAK_AUTHORITY
validate_https_url KEYCLOAK_INTERNAL_URL

reject_placeholder KEYCLOAK_REALM
validate_exact KEYCLOAK_ISLAMU_NGO_BLAZOR_CLIENT_ID islamu-ngo-blazor
validate_exact KEYCLOAK_ISLAMU_NGO_API_CLIENT_ID islamu-ngo-api
validate_exact KEYCLOAK_ISLAMU_NGO_AUDIENCE_MAPPER_NAME islamu-ngo-api-audience

realm=$(read_var KEYCLOAK_REALM)
authority=$(read_var KEYCLOAK_AUTHORITY)
case "$authority" in
  */realms/"$realm") ;;
  *) fail KEYCLOAK_AUTHORITY ;;
esac

echo "production validation ok"
