# =====================================================================
# Идемпотентный сидинг GitLab: админ + read-only пользователь + группа.
# Запускается внутри контейнера через gitlab-rails runner (см. load-and-run.sh).
# Повторный запуск безопасен — обновит пароли, не наплодит дублей.
#
#   docker exec -e ADMIN_PASSWORD=... -e USER_PASSWORD=... \
#     [-e ADMIN_USERNAME=...] [-e USER_USERNAME=...] \
#     gitlab gitlab-rails runner /tmp/seed-users.rb
#
# ВАЖНО про имена: GitLab резервирует ряд логинов (admin, api, users, root …).
# Логин 'admin' создать НЕЛЬЗЯ — поэтому дефолт админа = 'administrator'.
# Полноправный супер-пользователь 'root' и так есть (пароль = GITLAB_ROOT_PASSWORD).
# Пароли должны быть стойкими: GitLab отвергает короткие и «словарные».
# =====================================================================

admin_password = ENV['ADMIN_PASSWORD']
user_password  = ENV['USER_PASSWORD']
abort('ADMIN_PASSWORD не задан') if admin_password.to_s.empty?
abort('USER_PASSWORD не задан')  if user_password.to_s.empty?

RESERVED = %w[admin api root users dashboard groups projects help profile explore search public uploads].freeze
admin_username = (ENV['ADMIN_USERNAME'].to_s.strip.empty? ? 'administrator' : ENV['ADMIN_USERNAME'].strip)
user_username  = (ENV['USER_USERNAME'].to_s.strip.empty?  ? 'user'          : ENV['USER_USERNAME'].strip)
if RESERVED.include?(admin_username.downcase)
  warn "! Логин '#{admin_username}' зарезервирован GitLab — использую 'administrator'."
  admin_username = 'administrator'
end
if RESERVED.include?(user_username.downcase)
  warn "! Логин '#{user_username}' зарезервирован GitLab — использую 'altair-user'."
  user_username = 'altair-user'
end

# Создатель сущностей — встроенный root (он же админ). Через него Users/Groups
# CreateService проходят авторизацию и корректно строят namespace.
creator = User.find_by(username: 'root') || User.admins.first
abort('Не найден root/админ — не от кого создавать пользователей') unless creator

# Создать или обновить пользователя. Создание — через Users::CreateService
# (строит личный namespace; «голый» User.new этого не делает → Namespace can't be blank).
def upsert_user(creator, username:, name:, email:, password:, admin:)
  existing = User.find_by(username: username) || User.find_by(email: email)
  if existing
    existing.assign_attributes(password: password, password_confirmation: password, admin: admin)
    if existing.save
      puts "  (обновлён существующий '#{username}')"
      return existing
    end
    warn "! Не удалось обновить '#{username}': #{existing.errors.full_messages.join('; ')}"
    return nil
  end

  res = Users::CreateService.new(
    creator,
    username: username, name: name, email: email,
    password: password, password_confirmation: password,
    admin: admin, skip_confirmation: true
  ).execute

  user = res.respond_to?(:payload) ? res.payload[:user] : res
  return user if user.respond_to?(:persisted?) && user.persisted?

  msg = res.respond_to?(:message) ? res.message : nil
  msg ||= (user.respond_to?(:errors) ? user.errors.full_messages.join('; ') : res.inspect)
  warn "! Не удалось создать '#{username}': #{msg}"
  nil
end

admin = upsert_user(creator, username: admin_username, name: 'Administrator',
                    email: "#{admin_username}@altair.local", password: admin_password, admin: true)
puts "✓ админ:        #{admin_username}  (полные права)" if admin

ro = upsert_user(creator, username: user_username, name: 'Read Only',
                 email: "#{user_username}@altair.local", password: user_password, admin: false)
puts "✓ пользователь: #{user_username}  (обычная учётка)" if ro

# --- Группа 'altair': кладёшь проект сюда, и user сразу видит его read-only ---
owner = admin || creator
group = Group.find_by_full_path('altair')   # корректный API; колонки full_path в БД нет
unless group
  res = Groups::CreateService.new(
    owner, name: 'Altair', path: 'altair',
    visibility_level: Gitlab::VisibilityLevel::PRIVATE
  ).execute
  group = res.respond_to?(:payload) ? res.payload[:group] : res
end

if group.respond_to?(:persisted?) && group.persisted?
  group.add_owner(owner)   if owner
  group.add_reporter(ro)   if ro    # Reporter: клонировать/смотреть, БЕЗ push
  puts "✓ группа 'altair': owner=#{owner&.username}, reporter=#{ro&.username || '—'}"
else
  msg = group.respond_to?(:errors) ? group.errors.full_messages.join('; ') : group.inspect
  warn "! Группу 'altair' создать не удалось: #{msg}"
end

puts
puts 'Готово. Создай ПУСТОЙ проект в группе altair (UI) и залей код:'
puts '  git remote add origin http://<host>:<port>/altair/altair.git'
puts '  git push -u origin --all'
